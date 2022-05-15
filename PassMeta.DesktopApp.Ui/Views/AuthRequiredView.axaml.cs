namespace PassMeta.DesktopApp.Ui.Views
{
    using DesktopApp.Ui.ViewModels;
    using DesktopApp.Ui.Views.Base;
    using Avalonia.Markup.Xaml;
    
    public class AuthRequiredView : PageView<AuthRequiredViewModel>
    {
        public AuthRequiredView()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}