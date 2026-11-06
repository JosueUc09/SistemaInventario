using System.Text.Json.Serialization;

namespace InventarioEscolar.Models;

public class Mantenimiento
{
    [JsonPropertyName("id")]
    public int Id { get; set; }
    [JsonPropertyName("activo_id")]
    public int ActivoId { get; set; }
    [JsonPropertyName("activos")]
    public Activo? Activo { get; set; }
    [JsonPropertyName("fecha")]
    public DateTime Fecha { get; set; }
    [JsonPropertyName("tipo")]
    public string Tipo { get; set; } = "Preventivo";
    [JsonPropertyName("descripcion")]
    public string Descripcion { get; set; } = string.Empty;
    [JsonPropertyName("responsable")]
    public string Responsable { get; set; } = string.Empty;
    [JsonPropertyName("resultado")]
    public string Resultado { get; set; } = string.Empty;
}
