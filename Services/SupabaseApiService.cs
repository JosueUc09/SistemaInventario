using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using InventarioEscolar.Models;

namespace InventarioEscolar.Services;

public sealed class SupabaseApiService
{
    private readonly HttpClient _http;
    private readonly string _publishableKey;
    private readonly JsonSerializerOptions _json = new(JsonSerializerDefaults.Web)
    {
        PropertyNameCaseInsensitive = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    public SupabaseApiService(string url, string publishableKey)
    {
        _publishableKey = publishableKey;
        _http = new HttpClient { BaseAddress = new Uri(url.TrimEnd('/') + "/"), Timeout = TimeSpan.FromSeconds(30) };
    }

    public async Task<Usuario> SignInAsync(string email, string password)
    {
        // Una autenticación parcialmente completada no debe dejar credenciales en memoria.
        SessionService.Clear();
        try
        {
            using var request = NewRequest(HttpMethod.Post, "auth/v1/token?grant_type=password", authenticated: false);
            request.Content = JsonContent.Create(new { email, password }, options: _json);
            using var response = await _http.SendAsync(request);
            var auth = await ReadAsync<AuthResponse>(response);
            SessionService.AccessToken = auth.AccessToken;
            SessionService.RefreshToken = auth.RefreshToken;
            SessionService.AuthUserId = auth.User.Id;
            SessionService.ExpiresAt = DateTimeOffset.UtcNow.AddSeconds(auth.ExpiresIn);

            var profiles = await GetAsync<Usuario>($"perfiles?select=id,nombre,correo,role_id,activo,roles(id,nombre)&id=eq.{auth.User.Id}");
            var profile = profiles.SingleOrDefault() ?? throw new InvalidOperationException("La cuenta no tiene un perfil en la tabla perfiles.");
            if (!profile.Activo) throw new InvalidOperationException("La cuenta está desactivada.");
            SessionService.UsuarioActual = profile;
            return profile;
        }
        catch
        {
            SessionService.Clear();
            throw;
        }
    }

    public async Task SignOutAsync()
    {
        if (!string.IsNullOrWhiteSpace(SessionService.AccessToken))
        {
            using var request = NewRequest(HttpMethod.Post, "auth/v1/logout");
            using var response = await _http.SendAsync(request);
        }
        SessionService.Clear();
    }

    // PostgREST suele limitar el número de filas por respuesta. Lectura en páginas
    // ordenadas por clave primaria para no omitir registros después del primero.
    public async Task<List<Activo>> GetAssetsAsync()
    {
        const int pageSize = 200;
        var acumulados = new List<Activo>();
        for (var offset = 0; ; offset += pageSize)
        {
            await EnsureSessionAsync();
            using var request = NewRequest(HttpMethod.Get,
                "rest/v1/activos?select=id,numero_serie,numero_activo,nombre,descripcion,cantidad,categoria_id,ubicacion_id,estado_activo_id,activo_registro,creado_en,actualizado_en,categorias(id,nombre),ubicaciones(id,nombre),estados_activo(id,nombre,color)&activo_registro=eq.true&order=id.asc" + $"&limit={pageSize}&offset={offset}");
            using var response = await _http.SendAsync(request);
            var pagina = await ReadAsync<List<Activo>>(response);
            acumulados.AddRange(pagina);
            if (pagina.Count < pageSize) return acumulados;
        }
    }

    public Task<List<Categoria>> GetCategoriesAsync() => GetAsync<Categoria>("categorias?select=id,nombre&order=nombre.asc");
    public Task<List<Ubicacion>> GetLocationsAsync() => GetAsync<Ubicacion>("ubicaciones?select=id,nombre&order=nombre.asc");
    public Task<List<EstadoActivo>> GetAssetStatesAsync() => GetAsync<EstadoActivo>("estados_activo?select=id,nombre,color&order=id.asc");
    public Task<List<Role>> GetRolesAsync() => GetAsync<Role>("roles?select=id,nombre&order=nombre.asc");
    public Task<List<Usuario>> GetUsersAsync() => GetAsync<Usuario>("perfiles?select=id,nombre,correo,role_id,activo,creado_en,roles(id,nombre)&order=nombre.asc");
    public Task<List<Mantenimiento>> GetMaintenancesAsync() => GetAsync<Mantenimiento>(
        "mantenimientos?select=id,activo_id,fecha,tipo,descripcion,responsable,resultado,activos(id,numero_activo,nombre)&order=fecha.desc");
    public Task<List<Auditoria>> GetAuditAsync() => GetAsync<Auditoria>(
        "auditorias?select=id,usuario_id,modulo,accion,entidad,registro_id,valor_anterior,valor_nuevo,equipo,fecha,perfiles(nombre,correo)&order=fecha.desc&limit=500");

    public async Task<Activo?> GetAssetAsync(int id) => (await GetAsync<Activo>($"activos?select=*&id=eq.{id}")).SingleOrDefault();
    public async Task<Usuario?> GetUserAsync(Guid id) => (await GetAsync<Usuario>($"perfiles?select=id,nombre,correo,role_id,activo,roles(id,nombre)&id=eq.{id}")).SingleOrDefault();

    public async Task InsertAssetAsync(object values) => await SendDataAsync(HttpMethod.Post, "activos", values);
    public async Task UpdateAssetAsync(int id, object values) => await SendDataAsync(HttpMethod.Patch, $"activos?id=eq.{id}", values);
    public async Task InsertMaintenanceAsync(object values) => await SendDataAsync(HttpMethod.Post, "mantenimientos", values);
    public async Task UpdateUserAsync(Guid id, object values) => await SendDataAsync(HttpMethod.Patch, $"perfiles?id=eq.{id}", values);

    public async Task SaveUserAsync(Guid? id, string name, string email, string? password, int roleId, bool active)
    {
        await EnsureSessionAsync();
        using var request = NewRequest(HttpMethod.Post, "functions/v1/create-user");
        request.Content = JsonContent.Create(new { id, nombre = name, correo = email, password, role_id = roleId, activo = active }, options: _json);
        using var response = await _http.SendAsync(request);
        await EnsureSuccessAsync(response);
    }

    private async Task<List<T>> GetAsync<T>(string path)
    {
        await EnsureSessionAsync();
        using var request = NewRequest(HttpMethod.Get, "rest/v1/" + path);
        using var response = await _http.SendAsync(request);
        return await ReadAsync<List<T>>(response);
    }

    private async Task SendDataAsync(HttpMethod method, string path, object values)
    {
        await EnsureSessionAsync();
        using var request = NewRequest(method, "rest/v1/" + path);
        request.Headers.TryAddWithoutValidation("Prefer", "return=minimal");
        request.Content = new StringContent(JsonSerializer.Serialize(values, _json), Encoding.UTF8, "application/json");
        using var response = await _http.SendAsync(request);
        await EnsureSuccessAsync(response);
    }

    private HttpRequestMessage NewRequest(HttpMethod method, string path, bool authenticated = true)
    {
        var request = new HttpRequestMessage(method, path);
        request.Headers.TryAddWithoutValidation("apikey", _publishableKey);
        request.Headers.TryAddWithoutValidation("x-client-machine", Environment.MachineName);
        if (authenticated)
        {
            if (string.IsNullOrWhiteSpace(SessionService.AccessToken))
                throw new InvalidOperationException("La sesión no está activa. Inicie sesión nuevamente.");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", SessionService.AccessToken);
        }
        return request;
    }

    private async Task EnsureSessionAsync()
    {
        if (SessionService.ExpiresAt > DateTimeOffset.UtcNow.AddMinutes(2)) return;
        if (string.IsNullOrWhiteSpace(SessionService.RefreshToken))
            throw new InvalidOperationException("La sesión venció. Inicie sesión nuevamente.");

        using var request = NewRequest(HttpMethod.Post, "auth/v1/token?grant_type=refresh_token", authenticated: false);
        request.Content = JsonContent.Create(new { refresh_token = SessionService.RefreshToken }, options: _json);
        using var response = await _http.SendAsync(request);
        var auth = await ReadAsync<AuthResponse>(response);
        SessionService.AccessToken = auth.AccessToken;
        SessionService.RefreshToken = auth.RefreshToken;
        SessionService.AuthUserId = auth.User.Id;
        SessionService.ExpiresAt = DateTimeOffset.UtcNow.AddSeconds(auth.ExpiresIn);
    }

    private async Task<T> ReadAsync<T>(HttpResponseMessage response)
    {
        await EnsureSuccessAsync(response);
        return await response.Content.ReadFromJsonAsync<T>(_json)
               ?? throw new InvalidOperationException("Supabase devolvió una respuesta vacía.");
    }

    private static async Task EnsureSuccessAsync(HttpResponseMessage response)
    {
        if (response.IsSuccessStatusCode) return;
        var body = await response.Content.ReadAsStringAsync();
        var message = response.StatusCode switch
        {
            HttpStatusCode.Unauthorized => "Sesión vencida o credenciales incorrectas.",
            HttpStatusCode.Forbidden => "No tiene permiso para realizar esta acción.",
            HttpStatusCode.Conflict => "Ya existe un registro con esos datos.",
            _ => $"Supabase respondió {(int)response.StatusCode}: {body}"
        };
        throw new InvalidOperationException(message);
    }

    private sealed class AuthResponse
    {
        [JsonPropertyName("access_token")] public string AccessToken { get; set; } = string.Empty;
        [JsonPropertyName("refresh_token")] public string RefreshToken { get; set; } = string.Empty;
        [JsonPropertyName("expires_in")] public int ExpiresIn { get; set; } = 3600;
        [JsonPropertyName("user")] public AuthUser User { get; set; } = new();
    }

    private sealed class AuthUser
    {
        [JsonPropertyName("id")] public Guid Id { get; set; }
    }
}
