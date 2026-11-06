using Avalonia.Controls;
using Avalonia.Interactivity;
using InventarioEscolar.Models;
using InventarioEscolar.Services;

namespace InventarioEscolar.Views;

public partial class AssetEditorWindow : Window
{
    private readonly int? _assetId;

    public AssetEditorWindow(int? assetId = null)
    {
        InitializeComponent();
        _assetId = assetId;
        TitleText.Text = assetId.HasValue ? "Editar activo" : "Agregar activo";
        Opened += async (_, _) => await LoadAsync();
    }

    private async Task LoadAsync()
    {
        CategoriaCombo.ItemsSource = await AppServices.Supabase.GetCategoriesAsync();
        UbicacionCombo.ItemsSource = await AppServices.Supabase.GetLocationsAsync();
        EstadoCombo.ItemsSource = await AppServices.Supabase.GetAssetStatesAsync();

        if (_assetId is not int id)
        {
            CategoriaCombo.SelectedIndex = 0;
            UbicacionCombo.SelectedIndex = 0;
            EstadoCombo.SelectedIndex = 0;
            return;
        }

        var asset = await AppServices.Supabase.GetAssetAsync(id)
                    ?? throw new InvalidOperationException("El activo ya no existe.");
        SerieBox.Text = asset.NumeroSerie;
        ActivoBox.Text = asset.NumeroActivo;
        NombreBox.Text = asset.Nombre;
        DescripcionBox.Text = asset.Descripcion;
        CantidadBox.Value = asset.Cantidad;
        CategoriaCombo.SelectedItem = ((IEnumerable<Categoria>)CategoriaCombo.ItemsSource!).First(x => x.Id == asset.CategoriaId);
        UbicacionCombo.SelectedItem = ((IEnumerable<Ubicacion>)UbicacionCombo.ItemsSource!).First(x => x.Id == asset.UbicacionId);
        EstadoCombo.SelectedItem = ((IEnumerable<EstadoActivo>)EstadoCombo.ItemsSource!).First(x => x.Id == asset.EstadoActivoId);
    }

    private async void Save_OnClick(object? sender, RoutedEventArgs e)
    {
        StatusText.Text = string.Empty;
        if (string.IsNullOrWhiteSpace(SerieBox.Text) || string.IsNullOrWhiteSpace(ActivoBox.Text) ||
            string.IsNullOrWhiteSpace(NombreBox.Text) || CategoriaCombo.SelectedItem is not Categoria categoria ||
            UbicacionCombo.SelectedItem is not Ubicacion ubicacion || EstadoCombo.SelectedItem is not EstadoActivo estado)
        {
            StatusText.Text = "Complete todos los campos obligatorios.";
            return;
        }

        try
        {
            var values = new
            {
                numero_serie = SerieBox.Text.Trim(),
                numero_activo = ActivoBox.Text.Trim(),
                nombre = NombreBox.Text.Trim(),
                descripcion = DescripcionBox.Text?.Trim() ?? string.Empty,
                cantidad = (int)(CantidadBox.Value ?? 1),
                categoria_id = categoria.Id,
                ubicacion_id = ubicacion.Id,
                estado_activo_id = estado.Id,
                actualizado_en = DateTime.UtcNow
            };
            if (_assetId is int id) await AppServices.Supabase.UpdateAssetAsync(id, values);
            else await AppServices.Supabase.InsertAssetAsync(values);
            Close(true);
        }
        catch (Exception ex)
        {
            StatusText.Text = ex.Message;
        }
    }

    private void Cancel_OnClick(object? sender, RoutedEventArgs e) => Close(false);
}
