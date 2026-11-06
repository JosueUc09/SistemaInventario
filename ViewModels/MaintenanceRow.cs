namespace InventarioEscolar.ViewModels;

public class MaintenanceRow
{
    public int Id { get; set; }
    public string Fecha { get; set; } = string.Empty;
    public string Activo { get; set; } = string.Empty;
    public string Tipo { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;
    public string Responsable { get; set; } = string.Empty;
    public string Resultado { get; set; } = string.Empty;
}
