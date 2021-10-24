using Avalonia.Markup.Xaml;
using PassMeta.DesktopApp.Ui.ViewModels;
using PassMeta.DesktopApp.Ui.Views.Base;

namespace PassMeta.DesktopApp.Ui.Views
{
    public class AuthRequiredView : ViewPage<AuthRequiredViewModel>
    {
        public AuthRequiredView()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}