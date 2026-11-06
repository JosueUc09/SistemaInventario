using System.Text.Json.Serialization;
using System.Text.Json;

namespace InventarioEscolar.Models;

public class Auditoria
{
    [JsonPropertyName("id")]
    public long Id { get; set; }
    [JsonPropertyName("usuario_id")]
    public Guid? UsuarioId { get; set; }
    [JsonPropertyName("perfiles")]
    public Usuario? Usuario { get; set; }
    [JsonPropertyName("modulo")]
    public string Modulo { get; set; } = string.Empty;
    [JsonPropertyName("accion")]
    public string Accion { get; set; } = string.Empty;
    [JsonPropertyName("entidad")]
    public string Entidad { get; set; } = string.Empty;
    [JsonPropertyName("registro_id")]
    public string RegistroId { get; set; } = string.Empty;
    [JsonPropertyName("valor_anterior")]
    public JsonElement? ValorAnterior { get; set; }
    [JsonPropertyName("valor_nuevo")]
    public JsonElement? ValorNuevo { get; set; }
    [JsonPropertyName("equipo")]
    public string Equipo { get; set; } = Environment.MachineName;
    [JsonPropertyName("fecha")]
    public DateTime Fecha { get; set; } = DateTime.UtcNow;
}
