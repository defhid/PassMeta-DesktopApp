using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using PassMeta.DesktopApp.Ui.Models;

namespace PassMeta.DesktopApp.Ui.Views;

public class AuthView : ReactiveUserControl<AuthModel>
{
    public AuthView()
    {
        AvaloniaXamlLoader.Load(this);
    }

    private void Input_OnKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
            ViewModel!.SignInCommand.Execute(null);
    }

    private void LoginTextBox__OnAttachedToVisualTree(object? sender, VisualTreeAttachmentEventArgs e)
    {
        (sender as TextBox)?.Focus();
    }
}