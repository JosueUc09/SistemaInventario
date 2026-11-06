using System.Text.Json.Serialization;

namespace InventarioEscolar.Models;

public class Usuario
{
    [JsonPropertyName("id")]
    public Guid Id { get; set; }
    [JsonPropertyName("nombre")]
    public string Nombre { get; set; } = string.Empty;
    [JsonPropertyName("correo")]
    public string Correo { get; set; } = string.Empty;
    [JsonPropertyName("role_id")]
    public int RoleId { get; set; }
    [JsonPropertyName("roles")]
    public Role? Role { get; set; }
    [JsonPropertyName("activo")]
    public bool Activo { get; set; } = true;
    [JsonPropertyName("creado_en")]
    public DateTime CreadoEn { get; set; } = DateTime.UtcNow;
}
