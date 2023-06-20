using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using PassMeta.DesktopApp.Ui.Models.ViewModels.Pages;
using PassMeta.DesktopApp.Ui.Models.ViewModels.Pages.Account;

namespace PassMeta.DesktopApp.Ui.Views.Pages.Account;

public class AccountView : ReactiveUserControl<AccountPageModel>
{
    public AccountView()
    {
        AvaloniaXamlLoader.Load(this);
    }
}