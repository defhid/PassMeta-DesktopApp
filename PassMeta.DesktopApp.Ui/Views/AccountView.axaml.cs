namespace PassMeta.DesktopApp.Ui.Views
{
    using DesktopApp.Ui.ViewModels;
    using DesktopApp.Ui.Views.Base;
    using Avalonia.Markup.Xaml;
    
    public class AccountView : PageView<AccountViewModel>
    {
        public AccountView()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}