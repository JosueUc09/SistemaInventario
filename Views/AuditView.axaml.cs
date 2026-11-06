using Avalonia.Controls;
using Avalonia.Interactivity;
using InventarioEscolar.Services;
using InventarioEscolar.ViewModels;

namespace InventarioEscolar.Views;

public partial class AuditView : UserControl
{
    private List<AuditRow> _all = [];

    public AuditView()
    {
        InitializeComponent();
        Loaded += async (_, _) => await LoadAsync();
    }

    private async Task LoadAsync()
    {
        var entries = await AppServices.Supabase.GetAuditAsync();
        _all = entries.Select(x => new AuditRow
            {
                Id = x.Id,
                Fecha = x.Fecha.ToLocalTime().ToString("dd/MM/yyyy HH:mm"),
                Usuario = x.Usuario?.Nombre ?? "Sistema",
                Modulo = x.Modulo,
                Accion = x.Accion,
                Registro = x.Entidad + " #" + x.RegistroId,
                Equipo = x.Equipo,
                Cambios = x.ValorNuevo?.ToString() ?? x.ValorAnterior?.ToString() ?? "Sin detalle"
            }).ToList();
        ApplyFilter();
    }

    private void ApplyFilter()
    {
        var term = SearchBox.Text?.Trim() ?? string.Empty;
        var filtered = string.IsNullOrWhiteSpace(term) ? _all : _all.Where(x =>
            x.Usuario.Contains(term, StringComparison.OrdinalIgnoreCase) ||
            x.Modulo.Contains(term, StringComparison.OrdinalIgnoreCase) ||
            x.Accion.Contains(term, StringComparison.OrdinalIgnoreCase) ||
            x.Equipo.Contains(term, StringComparison.OrdinalIgnoreCase) ||
            x.Registro.Contains(term, StringComparison.OrdinalIgnoreCase)).ToList();
        AuditGrid.ItemsSource = filtered;
        CountText.Text = $"Mostrando {filtered.Count} movimientos (máximo 500 recientes)";
    }

    private void SearchBox_OnTextChanged(object? sender, TextChangedEventArgs e) => ApplyFilter();
    private async void Refresh_OnClick(object? sender, RoutedEventArgs e) => await LoadAsync();
}
