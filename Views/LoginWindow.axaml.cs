using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Interactivity;
using InventarioEscolar.Services;

namespace InventarioEscolar.Views;

public partial class LoginWindow : Window
{
    public LoginWindow() => InitializeComponent();

    private async void LoginButton_OnClick(object? sender, RoutedEventArgs e)
    {
        StatusText.Text = string.Empty;
        var email = UsernameBox.Text?.Trim() ?? string.Empty;
        var password = PasswordBox.Text ?? string.Empty;

        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
        {
            StatusText.Text = "Ingrese el correo y la contraseña.";
            return;
        }

        try
        {
            await AppServices.Supabase.SignInAsync(email, password);

            var mainWindow = new MainWindow();
            if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
                desktop.MainWindow = mainWindow;
            mainWindow.Show();
            Close();
        }
        catch (Exception ex)
        {
            StatusText.Text = $"No se pudo iniciar sesión: {ex.Message}";
        }
    }
}
