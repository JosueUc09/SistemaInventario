using Avalonia.Controls;
using Avalonia.Interactivity;
using InventarioEscolar.Models;
using InventarioEscolar.Services;

namespace InventarioEscolar.Views;

public partial class UsersView : UserControl
{
    private List<Usuario> _all = [];

    public UsersView()
    {
        InitializeComponent();
        Loaded += async (_, _) => await LoadAsync();
    }

    private async Task LoadAsync()
    {
        _all = await AppServices.Supabase.GetUsersAsync();
        ApplyFilter();
    }

    private void ApplyFilter()
    {
        var term = SearchBox.Text?.Trim() ?? string.Empty;
        var filtered = string.IsNullOrWhiteSpace(term) ? _all : _all.Where(x =>
            x.Nombre.Contains(term, StringComparison.OrdinalIgnoreCase) ||
            x.Correo.Contains(term, StringComparison.OrdinalIgnoreCase) ||
            (x.Role?.Nombre.Contains(term, StringComparison.OrdinalIgnoreCase) ?? false)).ToList();
        UsersGrid.ItemsSource = filtered;
        CountText.Text = $"{filtered.Count} usuarios";
    }

    private void SearchBox_OnTextChanged(object? sender, TextChangedEventArgs e) => ApplyFilter();

    private async void Add_OnClick(object? sender, RoutedEventArgs e)
    {
        if (TopLevel.GetTopLevel(this) is Window owner && await new UserEditorWindow().ShowDialog<bool>(owner))
            await LoadAsync();
    }

    private async void Edit_OnClick(object? sender, RoutedEventArgs e)
    {
        if (UsersGrid.SelectedItem is not Usuario selected || TopLevel.GetTopLevel(this) is not Window owner) return;
        if (await new UserEditorWindow(selected.Id).ShowDialog<bool>(owner)) await LoadAsync();
    }

    private async void Deactivate_OnClick(object? sender, RoutedEventArgs e)
    {
        if (UsersGrid.SelectedItem is not Usuario selected || TopLevel.GetTopLevel(this) is not Window owner) return;
        if (selected.Id == SessionService.UsuarioActual?.Id)
        {
            await MessageWindow.Show(owner, "Acción no permitida", "No puede desactivar la cuenta con la que inició sesión.");
            return;
        }

        if (!await ConfirmWindow.Show(owner, "Desactivar usuario", $"¿Desea desactivar a {selected.Nombre}?")) return;
        await AppServices.Supabase.UpdateUserAsync(selected.Id, new { activo = false });
        await LoadAsync();
    }
}
