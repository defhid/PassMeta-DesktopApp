using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using PassMeta.DesktopApp.Ui.ViewModels;

namespace PassMeta.DesktopApp.Ui.Views
{
    public class AuthRequiredView : ReactiveUserControl<AuthRequiredViewModel>
    {
        public AuthRequiredView()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}