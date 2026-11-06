using Avalonia.Controls;
using Avalonia.Interactivity;
using InventarioEscolar.Models;
using InventarioEscolar.Services;

namespace InventarioEscolar.Views;

public partial class InventoryView : UserControl
{
    private readonly IndiceInventario _indice = new();

    public InventoryView()
    {
        InitializeComponent();
        Loaded += async (_, _) => await CargarSeguroAsync();
        var puedeEditar = SessionService.UsuarioActual?.Role?.Nombre is "Administrador" or "Administrativo";
        AddButton.IsVisible = puedeEditar;
        EditButton.IsVisible = puedeEditar;
        DeleteButton.IsVisible = puedeEditar;
    }

    private async Task LoadAsync()
    {
        _indice.Actualizar(await AppServices.Supabase.GetAssetsAsync());
        ApplyFilter();
    }

    private async Task CargarSeguroAsync()
    {
        try { await LoadAsync(); }
        catch (Exception ex)
        {
            if (TopLevel.GetTopLevel(this) is Window owner)
                await MessageWindow.Show(owner, "No se pudo cargar inventario", ex.Message);
        }
    }

    private void ApplyFilter()
    {
        var term = SearchBox.Text?.Trim() ?? string.Empty;
        var filtrados = _indice.Buscar(term);
        InventoryGrid.ItemsSource = filtrados;
        CountText.Text = $"{filtrados.Count} de {_indice.Cantidad} activos · búsqueda exacta por hash o filtro lineal";
    }

    private void SearchBox_OnTextChanged(object? sender, TextChangedEventArgs e) => ApplyFilter();
    private async void Refresh_OnClick(object? sender, RoutedEventArgs e) => await CargarSeguroAsync();

    private async void Add_OnClick(object? sender, RoutedEventArgs e)
    {
        if (TopLevel.GetTopLevel(this) is Window owner)
        {
            var saved = await new AssetEditorWindow().ShowDialog<bool>(owner);
            if (saved) await CargarSeguroAsync();
        }
    }

    private async void Edit_OnClick(object? sender, RoutedEventArgs e)
    {
        if (InventoryGrid.SelectedItem is not Activo selected) return;
        if (TopLevel.GetTopLevel(this) is Window owner)
        {
            var saved = await new AssetEditorWindow(selected.Id).ShowDialog<bool>(owner);
            if (saved) await CargarSeguroAsync();
        }
    }

    private async void Delete_OnClick(object? sender, RoutedEventArgs e)
    {
        if (InventoryGrid.SelectedItem is not Activo selected || TopLevel.GetTopLevel(this) is not Window owner) return;
        var confirmed = await ConfirmWindow.Show(owner, "Eliminar activo", $"¿Desea desactivar el activo {selected.NumeroActivo} - {selected.Nombre}?");
        if (!confirmed) return;

        await AppServices.Supabase.UpdateAssetAsync(selected.Id, new { activo_registro = false, actualizado_en = DateTime.UtcNow });
        await CargarSeguroAsync();
    }
}
