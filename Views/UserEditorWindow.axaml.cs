using Avalonia.Controls;
using Avalonia.Interactivity;
using InventarioEscolar.Models;
using InventarioEscolar.Services;

namespace InventarioEscolar.Views;

public partial class UserEditorWindow : Window
{
    private readonly Guid? _userId;

    public UserEditorWindow(Guid? userId = null)
    {
        InitializeComponent();
        _userId = userId;
        TitleText.Text = userId.HasValue ? "Editar usuario" : "Agregar usuario";
        PasswordLabel.Text = userId.HasValue ? "Nueva contraseña (opcional)" : "Contraseña";
        Opened += async (_, _) => await LoadAsync();
    }

    private async Task LoadAsync()
    {
        var roles = await AppServices.Supabase.GetRolesAsync();
        RoleCombo.ItemsSource = roles;
        RoleCombo.SelectedIndex = 0;

        if (_userId is not Guid id) return;
        var user = await AppServices.Supabase.GetUserAsync(id)
                   ?? throw new InvalidOperationException("El usuario ya no existe.");
        NameBox.Text = user.Nombre;
        EmailBox.Text = user.Correo;
        ActiveCheck.IsChecked = user.Activo;
        RoleCombo.SelectedItem = roles.First(x => x.Id == user.RoleId);
    }

    private async void Save_OnClick(object? sender, RoutedEventArgs e)
    {
        StatusText.Text = string.Empty;
        var password = PasswordBox.Text ?? string.Empty;
        if (string.IsNullOrWhiteSpace(NameBox.Text) || string.IsNullOrWhiteSpace(EmailBox.Text) ||
            RoleCombo.SelectedItem is not Role role)
        {
            StatusText.Text = "Complete todos los campos obligatorios.";
            return;
        }

        if (!_userId.HasValue && password.Length < 8 || _userId.HasValue && password.Length is > 0 and < 8)
        {
            StatusText.Text = "La contraseña debe contener al menos 8 caracteres.";
            return;
        }

        try
        {
            await AppServices.Supabase.SaveUserAsync(_userId, NameBox.Text.Trim(), EmailBox.Text.Trim(),
                string.IsNullOrEmpty(password) ? null : password, role.Id, ActiveCheck.IsChecked == true);
            Close(true);
        }
        catch (Exception ex)
        {
            StatusText.Text = ex.Message;
        }
    }

    private void Cancel_OnClick(object? sender, RoutedEventArgs e) => Close(false);
}
