using System.Text.Json.Serialization;

namespace InventarioEscolar.Models;

public class EstadoActivo
{
    [JsonPropertyName("id")]
    public int Id { get; set; }
    [JsonPropertyName("nombre")]
    public string Nombre { get; set; } = string.Empty;
    [JsonPropertyName("color")]
    public string Color { get; set; } = "#6B7280";
}
