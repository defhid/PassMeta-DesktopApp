namespace PassMeta.DesktopApp.Ui.Views
{
    using ViewModels;
    using Base;
    using Avalonia.Markup.Xaml;
    
    public class AccountView : PageView<AccountViewModel>
    {
        public AccountView()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}