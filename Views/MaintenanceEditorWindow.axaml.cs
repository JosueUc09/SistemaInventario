using Avalonia.Controls;
using Avalonia.Interactivity;
using InventarioEscolar.Models;
using InventarioEscolar.Services;

namespace InventarioEscolar.Views;

public partial class MaintenanceEditorWindow : Window
{
    private sealed class AssetOption
    {
        public int Id { get; init; }
        public string Nombre { get; init; } = string.Empty;
    }

    public MaintenanceEditorWindow()
    {
        InitializeComponent();
        DateBox.Text = DateTime.Today.ToString("dd/MM/yyyy");
        ResponsibleBox.Text = SessionService.UsuarioActual?.Nombre;
        Opened += async (_, _) => await LoadAsync();
    }

    private async Task LoadAsync()
    {
        AssetCombo.ItemsSource = (await AppServices.Supabase.GetAssetsAsync())
            .OrderBy(x => x.NumeroActivo)
            .Select(x => new AssetOption { Id = x.Id, Nombre = x.NumeroActivo + " - " + x.Nombre }).ToList();
        AssetCombo.SelectedIndex = 0;
    }

    private async void Save_OnClick(object? sender, RoutedEventArgs e)
    {
        if (AssetCombo.SelectedItem is not AssetOption asset ||
            !DateTime.TryParseExact(DateBox.Text, "dd/MM/yyyy", null, System.Globalization.DateTimeStyles.None, out var date) ||
            TypeCombo.SelectedItem is not ComboBoxItem type || string.IsNullOrWhiteSpace(DescriptionBox.Text) ||
            string.IsNullOrWhiteSpace(ResponsibleBox.Text))
        {
            StatusText.Text = "Complete los campos obligatorios.";
            return;
        }

        await AppServices.Supabase.InsertMaintenanceAsync(new
        {
            activo_id = asset.Id,
            fecha = DateTime.SpecifyKind(date, DateTimeKind.Utc),
            tipo = type.Content?.ToString() ?? "Preventivo",
            descripcion = DescriptionBox.Text.Trim(),
            responsable = ResponsibleBox.Text.Trim(),
            resultado = ResultBox.Text?.Trim() ?? string.Empty
        });
        Close(true);
    }

    private void Cancel_OnClick(object? sender, RoutedEventArgs e) => Close(false);
}
