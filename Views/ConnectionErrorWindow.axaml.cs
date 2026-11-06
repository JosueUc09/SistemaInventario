using Avalonia.Controls;
using Avalonia.Interactivity;

namespace InventarioEscolar.Views;

public partial class ConnectionErrorWindow : Window
{
    public ConnectionErrorWindow(string message)
    {
        InitializeComponent();
        ErrorText.Text = message;
    }

    private void Close_OnClick(object? sender, RoutedEventArgs e) => Close();
}
