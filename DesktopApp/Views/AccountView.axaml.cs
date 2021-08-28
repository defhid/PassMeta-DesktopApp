using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using DesktopApp.ViewModels;

namespace DesktopApp.Views
{
    public class AccountView : ReactiveUserControl<AccountViewModel>
    {
        public AccountView()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}