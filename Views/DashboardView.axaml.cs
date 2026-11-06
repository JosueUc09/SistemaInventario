using Avalonia.Controls;
using Avalonia.Interactivity;
using InventarioEscolar.Services;

namespace InventarioEscolar.Views;

public partial class DashboardView : UserControl
{
    public DashboardView()
    {
        InitializeComponent();
        MachineText.Text = Environment.MachineName;
        Loaded += OnLoaded;
    }

    private async void OnLoaded(object? sender, RoutedEventArgs e)
    {
        try
        {
            var activos = await AppServices.Supabase.GetAssetsAsync();
            var indice = new IndiceInventario();
            indice.Actualizar(activos);
            var resumen = AnaliticaInventario.ResumirParalelo(activos);
            TotalText.Text = resumen.Total.ToString();
            DisponibleText.Text = resumen.Disponibles.ToString();
            RevisarText.Text = resumen.PorRevisar.ToString();
            MantenimientoText.Text = resumen.EnMantenimiento.ToString();
            RecentGrid.ItemsSource = indice.ListarOrdenadosPorCodigo().TakeLast(8).ToList();
            // Cola FIFO calculada a partir de reglas Horn; muestra los tres primeros.
            var pendientes = indice.RevisionesPendientes;
            var proximos = new List<string>();
            while (proximos.Count < 3 && indice.SiguienteRevision(out var activo))
                if (activo is not null) proximos.Add(activo.NumeroActivo);
            RevisionQueueText.Text = pendientes == 0
                ? "Sin revisiones pendientes"
                : $"{pendientes} activos priorizados. Primeros: {string.Join(", ", proximos)}";
        }
        catch (Exception ex)
        {
            if (TopLevel.GetTopLevel(this) is Window owner)
                await MessageWindow.Show(owner, "Error de carga", ex.Message);
        }
    }
}
