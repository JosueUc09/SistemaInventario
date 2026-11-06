using Avalonia.Controls;
using Avalonia.Interactivity;
using InventarioEscolar.Services;

namespace InventarioEscolar.Views;

public partial class ChartsView : UserControl
{
    public ChartsView()
    {
        InitializeComponent();
        Loaded += OnLoaded;
    }

    private async void OnLoaded(object? sender, RoutedEventArgs e)
    {
        var activos = await AppServices.Supabase.GetAssetsAsync();
        var grouped = activos.GroupBy(x => x.EstadoActivo?.Nombre ?? "Sin estado")
            .ToDictionary(x => x.Key, x => x.Sum(a => a.Cantidad));
        var total = grouped.Values.Sum();
        var max = Math.Max(total, 1);

        SetBar(AvailableBar, AvailableLabel, grouped.GetValueOrDefault("Disponible"), max);
        SetBar(ReviewBar, ReviewLabel, grouped.GetValueOrDefault("Por revisar"), max);
        SetBar(MaintenanceBar, MaintenanceLabel, grouped.GetValueOrDefault("En mantenimiento"), max);
        SetBar(OutBar, OutLabel, grouped.GetValueOrDefault("Fuera de servicio"), max);

        TotalLabel.Text = total.ToString();
        LocationsLabel.Text = (await AppServices.Supabase.GetLocationsAsync()).Count.ToString();
        MaintenancesLabel.Text = (await AppServices.Supabase.GetMaintenancesAsync()).Count.ToString();
    }

    private static void SetBar(ProgressBar bar, TextBlock label, int value, int max)
    {
        bar.Maximum = max;
        bar.Value = value;
        label.Text = value.ToString();
    }
}
