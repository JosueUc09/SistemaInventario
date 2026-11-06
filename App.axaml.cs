using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using InventarioEscolar.Services;
using InventarioEscolar.Views;

namespace InventarioEscolar;

public partial class App : Application
{
    public override void Initialize() => AvaloniaXamlLoader.Load(this);

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            try
            {
                AppServices.Configure();
                desktop.MainWindow = new LoginWindow();
            }
            catch (Exception ex)
            {
                desktop.MainWindow = new ConnectionErrorWindow(ex.Message);
            }
        }

        base.OnFrameworkInitializationCompleted();
    }
}
