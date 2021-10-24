using Avalonia.Markup.Xaml;
using PassMeta.DesktopApp.Ui.ViewModels;
using PassMeta.DesktopApp.Ui.Views.Base;

namespace PassMeta.DesktopApp.Ui.Views
{
    public class AccountView : ViewPage<AccountViewModel>
    {
        public AccountView()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}