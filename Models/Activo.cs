using System.Text.Json.Serialization;

namespace InventarioEscolar.Models;

public class Activo
{
    [JsonPropertyName("id")]
    public int Id { get; set; }
    [JsonPropertyName("numero_serie")]
    public string NumeroSerie { get; set; } = string.Empty;
    [JsonPropertyName("numero_activo")]
    public string NumeroActivo { get; set; } = string.Empty;
    [JsonPropertyName("nombre")]
    public string Nombre { get; set; } = string.Empty;
    [JsonPropertyName("descripcion")]
    public string Descripcion { get; set; } = string.Empty;
    [JsonPropertyName("cantidad")]
    public int Cantidad { get; set; } = 1;
    [JsonPropertyName("categoria_id")]
    public int CategoriaId { get; set; }
    [JsonPropertyName("categorias")]
    public Categoria? Categoria { get; set; }
    [JsonPropertyName("ubicacion_id")]
    public int UbicacionId { get; set; }
    [JsonPropertyName("ubicaciones")]
    public Ubicacion? Ubicacion { get; set; }
    [JsonPropertyName("estado_activo_id")]
    public int EstadoActivoId { get; set; }
    [JsonPropertyName("estados_activo")]
    public EstadoActivo? EstadoActivo { get; set; }
    [JsonPropertyName("activo_registro")]
    public bool ActivoRegistro { get; set; } = true;
    [JsonPropertyName("creado_en")]
    public DateTime CreadoEn { get; set; } = DateTime.UtcNow;
    [JsonPropertyName("actualizado_en")]
    public DateTime? ActualizadoEn { get; set; }
}
