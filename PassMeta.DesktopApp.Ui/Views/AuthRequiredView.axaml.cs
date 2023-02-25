namespace PassMeta.DesktopApp.Ui.Views;

using ViewModels;
using Base;
using Avalonia.Markup.Xaml;
    
public class AuthRequiredView : PageView<AuthRequiredViewModel>
{
    public AuthRequiredView()
    {
        AvaloniaXamlLoader.Load(this);
    }
}