using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using PassMeta.DesktopApp.Ui.Models;
using PassMeta.DesktopApp.Ui.Models.ViewModels.Pages;

namespace PassMeta.DesktopApp.Ui.Views.Pages.Account;

public class AccountView : ReactiveUserControl<AccountPageModel>
{
    public AccountView()
    {
        AvaloniaXamlLoader.Load(this);
    }
}