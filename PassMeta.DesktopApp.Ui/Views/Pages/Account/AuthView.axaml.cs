using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.ReactiveUI;
using PassMeta.DesktopApp.Ui.Models.ViewModels.Pages.Account;

namespace PassMeta.DesktopApp.Ui.Views.Pages.Account;

public partial class AuthView : ReactiveUserControl<AuthPageModel>
{
    public AuthView()
    {
        InitializeComponent();
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