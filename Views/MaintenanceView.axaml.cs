using Avalonia.Controls;
using Avalonia.Interactivity;
using InventarioEscolar.Services;
using InventarioEscolar.ViewModels;

namespace InventarioEscolar.Views;

public partial class MaintenanceView : UserControl
{
    private List<MaintenanceRow> _all = [];

    public MaintenanceView()
    {
        InitializeComponent();
        Loaded += async (_, _) => await LoadAsync();
        AddButton.IsVisible = SessionService.UsuarioActual?.Role?.Nombre is "Administrador" or "Administrativo";
    }

    private async Task LoadAsync()
    {
        var entries = await AppServices.Supabase.GetMaintenancesAsync();
        _all = entries.Select(x => new MaintenanceRow
            {
                Id = x.Id,
                Fecha = x.Fecha.ToString("dd/MM/yyyy"),
                Activo = x.Activo!.NumeroActivo + " - " + x.Activo.Nombre,
                Tipo = x.Tipo,
                Descripcion = x.Descripcion,
                Responsable = x.Responsable,
                Resultado = x.Resultado
            }).ToList();
        ApplyFilter();
    }

    private void ApplyFilter()
    {
        var term = SearchBox.Text?.Trim() ?? string.Empty;
        var filtered = string.IsNullOrWhiteSpace(term) ? _all : _all.Where(x =>
            x.Activo.Contains(term, StringComparison.OrdinalIgnoreCase) ||
            x.Responsable.Contains(term, StringComparison.OrdinalIgnoreCase) ||
            x.Tipo.Contains(term, StringComparison.OrdinalIgnoreCase)).ToList();
        MaintenanceGrid.ItemsSource = filtered;
        CountText.Text = $"{filtered.Count} mantenimientos registrados";
    }

    private void SearchBox_OnTextChanged(object? sender, TextChangedEventArgs e) => ApplyFilter();

    private async void Add_OnClick(object? sender, RoutedEventArgs e)
    {
        if (TopLevel.GetTopLevel(this) is Window owner && await new MaintenanceEditorWindow().ShowDialog<bool>(owner))
            await LoadAsync();
    }
}
