using InventarioEscolar.Models;

namespace InventarioEscolar.Services;

/// <summary>
/// Índices temporales construidos exclusivamente a partir de registros de Supabase.
/// El repositorio remoto es la fuente de verdad; Actualizar() vuelve a cargar estos índices.
/// </summary>
public sealed class IndiceInventario
{
    private readonly List<Activo> _lista = new();
    private readonly Dictionary<string, Activo> _porCodigo = new(StringComparer.OrdinalIgnoreCase);
    private readonly SortedDictionary<string, Activo> _ordenados = new(StringComparer.OrdinalIgnoreCase);
    private readonly Queue<Activo> _colaRevision = new();

    public int Cantidad => _lista.Count;
    public int RevisionesPendientes => _colaRevision.Count;
    public IReadOnlyList<Activo> Listado => _lista;

    public void Actualizar(IEnumerable<Activo> activos)
    {
        ArgumentNullException.ThrowIfNull(activos);
        // Construcción atómica: si hay un código duplicado, el índice anterior no se altera.
        var lista = activos.ToList();
        var hash = new Dictionary<string, Activo>(StringComparer.OrdinalIgnoreCase);
        var arbol = new SortedDictionary<string, Activo>(StringComparer.OrdinalIgnoreCase);
        var cola = new Queue<Activo>();
        foreach (var activo in lista)
        {
            if (string.IsNullOrWhiteSpace(activo.NumeroActivo) || !hash.TryAdd(activo.NumeroActivo, activo))
                throw new ArgumentException($"Código de activo inválido o duplicado: {activo.NumeroActivo}");
            arbol.Add(activo.NumeroActivo, activo);
            if (ReglasInventario.RequiereRevision(activo)) cola.Enqueue(activo);
        }
        _lista.Clear(); _lista.AddRange(lista);
        _porCodigo.Clear(); foreach (var par in hash) _porCodigo.Add(par.Key, par.Value);
        _ordenados.Clear(); foreach (var par in arbol) _ordenados.Add(par.Key, par.Value);
        _colaRevision.Clear(); foreach (var a in cola) _colaRevision.Enqueue(a);
    }

    // Complejidad promedio O(1) para búsquedas exactas por código.
    public Activo? BuscarPorCodigo(string codigo) =>
        _porCodigo.GetValueOrDefault((codigo ?? string.Empty).Trim());

    // Complejidad O(n) cuando se buscan fragmentos en diferentes atributos.
    public List<Activo> Buscar(string? termino)
    {
        var t = termino?.Trim() ?? string.Empty;
        if (t.Length == 0) return _lista.ToList();
        var exacto = BuscarPorCodigo(t);
        if (exacto is not null) return new List<Activo> { exacto };
        return _lista.Where(a => a.NumeroActivo.Contains(t, StringComparison.OrdinalIgnoreCase)
          || a.NumeroSerie.Contains(t, StringComparison.OrdinalIgnoreCase)
          || a.Nombre.Contains(t, StringComparison.OrdinalIgnoreCase)
          || a.Descripcion.Contains(t, StringComparison.OrdinalIgnoreCase)
          || (a.Ubicacion?.Nombre.Contains(t, StringComparison.OrdinalIgnoreCase) ?? false)).ToList();
    }

    // SortedDictionary es un árbol balanceado: O(log n) en inserción y búsqueda.
    public List<Activo> ListarOrdenadosPorCodigo() => _ordenados.Values.ToList();

    // Queue aplica FIFO; se usa para preparar el orden de revisión, no como persistencia.
    public bool SiguienteRevision(out Activo? activo)
    {
        if (_colaRevision.TryDequeue(out var pendiente)) { activo = pendiente; return true; }
        activo = null; return false;
    }
}
