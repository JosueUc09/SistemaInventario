using InventarioEscolar.Models;

namespace InventarioEscolar.Services;

/// <summary>Reglas deterministas tipo Horn: hechos sobre el estado implican recomendaciones.
/// Un equivalente ejecutable en Prolog figura en ejemplos/01_reglas_inventario.pl.</summary>
public static class ReglasInventario
{
    public static bool RequiereRevision(Activo activo) =>
        activo.EstadoActivo?.Nombre is "Por revisar" or "Fuera de servicio";

    public static bool RequiereMantenimiento(Activo activo) =>
        activo.EstadoActivo?.Nombre == "En mantenimiento";

    public static string Diagnostico(Activo activo) =>
        RequiereRevision(activo) ? "Priorizar revisión" :
        RequiereMantenimiento(activo) ? "Dar seguimiento al mantenimiento" :
        "Sin alerta por estado";
}
