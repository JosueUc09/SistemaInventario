using Avalonia.Controls;
using Avalonia.Interactivity;

namespace InventarioEscolar.Views;

public partial class MessageWindow : Window
{
    private MessageWindow(string title, string message)
    {
        InitializeComponent();
        Title = title;
        TitleText.Text = title;
        MessageText.Text = message;
    }

    public static Task Show(Window owner, string title, string message) =>
        new MessageWindow(title, message).ShowDialog(owner);

    private void Accept_OnClick(object? sender, RoutedEventArgs e) => Close();
}
