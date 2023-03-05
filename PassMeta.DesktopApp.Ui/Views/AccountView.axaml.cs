using Avalonia.ReactiveUI;
using Avalonia.Markup.Xaml;
using PassMeta.DesktopApp.Ui.Models;

namespace PassMeta.DesktopApp.Ui.Views;

public class AccountView : ReactiveUserControl<AccountModel>
{
    public AccountView()
    {
        AvaloniaXamlLoader.Load(this);
    }
}