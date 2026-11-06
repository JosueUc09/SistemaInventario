using System.Text.Json.Serialization;

namespace InventarioEscolar.Models;

public class Role
{
    [JsonPropertyName("id")]
    public int Id { get; set; }
    [JsonPropertyName("nombre")]
    public string Nombre { get; set; } = string.Empty;
}
