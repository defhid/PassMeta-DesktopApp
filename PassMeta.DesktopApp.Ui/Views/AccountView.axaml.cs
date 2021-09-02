using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using PassMeta.DesktopApp.Ui.ViewModels;

namespace PassMeta.DesktopApp.Ui.Views
{
    public class AccountView : ReactiveUserControl<AccountViewModel>
    {
        public AccountView()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}