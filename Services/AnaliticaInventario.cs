using System.Collections.Concurrent;
using InventarioEscolar.Models;

namespace InventarioEscolar.Services;

public readonly record struct ResumenInventario(int Total, int Disponibles, int PorRevisar,
    int EnMantenimiento, int FueraDeServicio)
{
    public static ResumenInventario operator +(ResumenInventario a, ResumenInventario b) =>
        new(a.Total+b.Total,a.Disponibles+b.Disponibles,a.PorRevisar+b.PorRevisar,
            a.EnMantenimiento+b.EnMantenimiento,a.FueraDeServicio+b.FueraDeServicio);
}

/// <summary>Funciones sin efectos secundarios: estilo funcional. Para lotes grandes,
/// se dividen rangos y se procesan en paralelo en el equipo local (NO grid real).</summary>
public static class AnaliticaInventario
{
    public static ResumenInventario Resumir(IEnumerable<Activo> activos)
    {
        var resultado = new ResumenInventario();
        foreach(var a in activos) resultado += Contar(a);
        return resultado;
    }

    public static ResumenInventario ResumirParalelo(IReadOnlyList<Activo> activos)
    {
        ArgumentNullException.ThrowIfNull(activos);
        if (activos.Count < 250) return Resumir(activos);
        var parciales = new ConcurrentBag<ResumenInventario>();
        var intervalo = Math.Max(50, activos.Count / Math.Max(1, Environment.ProcessorCount));
        Parallel.ForEach(Partitioner.Create(0, activos.Count, intervalo), rango =>
        {
            var local = new ResumenInventario();
            for (int i = rango.Item1; i < rango.Item2; i++) local += Contar(activos[i]);
            parciales.Add(local);
        });
        return ResumirParciales(parciales);
    }

    public static List<Activo> FiltrarDisponibles(IEnumerable<Activo> activos) =>
        activos.Where(a => a.EstadoActivo?.Nombre == "Disponible")
               .OrderBy(a => a.NumeroActivo, StringComparer.OrdinalIgnoreCase).ToList();

    private static ResumenInventario ResumirParciales(IEnumerable<ResumenInventario> items)
    {
        var suma = new ResumenInventario();
        foreach (var x in items) suma += x;
        return suma;
    }

    private static ResumenInventario Contar(Activo a)
    {
        var cantidad = a.Cantidad;
        return new ResumenInventario(cantidad,
            a.EstadoActivo?.Nombre == "Disponible" ? cantidad : 0,
            a.EstadoActivo?.Nombre == "Por revisar" ? cantidad : 0,
            a.EstadoActivo?.Nombre == "En mantenimiento" ? cantidad : 0,
            a.EstadoActivo?.Nombre == "Fuera de servicio" ? cantidad : 0);
    }
}
