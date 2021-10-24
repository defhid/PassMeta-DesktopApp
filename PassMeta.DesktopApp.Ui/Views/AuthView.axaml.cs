using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using PassMeta.DesktopApp.Ui.ViewModels;
using PassMeta.DesktopApp.Ui.Views.Base;

namespace PassMeta.DesktopApp.Ui.Views
{
    public class AuthView : ViewPage<AuthViewModel>
    {
        public AuthView()
        {
            AvaloniaXamlLoader.Load(this);
        }

        private void Input_OnKeyDown(object? sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                DataContext!.SignInCommand.Execute(null);
        }

        private void LoginTextBox__OnAttachedToVisualTree(object? sender, VisualTreeAttachmentEventArgs e)
        {
            (sender as TextBox)?.Focus();
        }
    }
}