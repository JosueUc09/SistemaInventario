using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Interactivity;
using InventarioEscolar.Services;

namespace InventarioEscolar.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        CurrentUserText.Text = SessionService.UsuarioActual?.Nombre ?? "Usuario";
        CurrentRoleText.Text = SessionService.UsuarioActual?.Role?.Nombre ?? string.Empty;
        UsuariosButton.IsVisible = SessionService.EsAdministrador;
        AuditoriaButton.IsVisible = SessionService.EsAdministrador;
        Navigate("Inicio", new DashboardView());
    }

    private void Navigate(string title, Control view)
    {
        PageTitle.Text = title;
        PageContent.Content = view;
    }

    private void Inicio_OnClick(object? sender, RoutedEventArgs e) => Navigate("Inicio", new DashboardView());
    private void Usuarios_OnClick(object? sender, RoutedEventArgs e) => Navigate("Usuarios", new UsersView());
    private void Inventario_OnClick(object? sender, RoutedEventArgs e) => Navigate("Administrar inventario", new InventoryView());
    private void Mantenimientos_OnClick(object? sender, RoutedEventArgs e) => Navigate("Mantenimientos", new MaintenanceView());
    private void Auditoria_OnClick(object? sender, RoutedEventArgs e) => Navigate("Auditoría", new AuditView());
    private void Graficos_OnClick(object? sender, RoutedEventArgs e) => Navigate("Gráficos", new ChartsView());

    private async void CerrarSesion_OnClick(object? sender, RoutedEventArgs e)
    {
        await AppServices.Supabase.SignOutAsync();
        var login = new LoginWindow();
        if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            desktop.MainWindow = login;
        login.Show();
        Close();
    }
}
