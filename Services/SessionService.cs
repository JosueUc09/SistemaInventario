using InventarioEscolar.Models;

namespace InventarioEscolar.Services;

public static class SessionService
{
    public static Usuario? UsuarioActual { get; set; }
    public static string? AccessToken { get; set; }
    public static string? RefreshToken { get; set; }
    public static Guid? AuthUserId { get; set; }
    public static DateTimeOffset ExpiresAt { get; set; }
    public static bool EsAdministrador => UsuarioActual?.Role?.Nombre == "Administrador";

    public static void Clear()
    {
        UsuarioActual = null;
        AccessToken = null;
        RefreshToken = null;
        AuthUserId = null;
        ExpiresAt = default;
    }
}
