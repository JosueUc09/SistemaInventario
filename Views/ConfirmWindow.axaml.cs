using Avalonia.Controls;
using Avalonia.Interactivity;

namespace InventarioEscolar.Views;

public partial class ConfirmWindow : Window
{
    private ConfirmWindow(string title, string message)
    {
        InitializeComponent();
        Title = title;
        TitleText.Text = title;
        MessageText.Text = message;
    }

    public static Task<bool> Show(Window owner, string title, string message) =>
        new ConfirmWindow(title, message).ShowDialog<bool>(owner);

    private void Yes_OnClick(object? sender, RoutedEventArgs e) => Close(true);
    private void No_OnClick(object? sender, RoutedEventArgs e) => Close(false);
}
