namespace InventarioEscolar.ViewModels;

public class AuditRow
{
    public long Id { get; set; }
    public string Fecha { get; set; } = string.Empty;
    public string Usuario { get; set; } = string.Empty;
    public string Modulo { get; set; } = string.Empty;
    public string Accion { get; set; } = string.Empty;
    public string Registro { get; set; } = string.Empty;
    public string Equipo { get; set; } = string.Empty;
    public string Cambios { get; set; } = string.Empty;
}
