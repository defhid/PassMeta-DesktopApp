using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using PassMeta.DesktopApp.Ui.Models;
using PassMeta.DesktopApp.Ui.Models.ViewModels.Pages;

namespace PassMeta.DesktopApp.Ui.Views.Pages.Account;

public class AuthView : ReactiveUserControl<AuthPageModel>
{
    public AuthView()
    {
        AvaloniaXamlLoader.Load(this);
    }

    private void Input_OnKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
            ViewModel!.LogInCommand.Execute(null);
    }

    private void LoginTextBox__OnAttachedToVisualTree(object? sender, VisualTreeAttachmentEventArgs e)
    {
        (sender as TextBox)?.Focus();
    }
}