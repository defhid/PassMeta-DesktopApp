using System;
using System.ComponentModel;
using System.Linq;
using Avalonia;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using PassMeta.DesktopApp.Common.Abstractions.App;
using PassMeta.DesktopApp.Common.Abstractions.PassFileContext;
using PassMeta.DesktopApp.Common.Abstractions.Services;
using PassMeta.DesktopApp.Common.Extensions;
using PassMeta.DesktopApp.Ui.Models.ViewModels.Base;
using PassMeta.DesktopApp.Ui.Models.ViewModels.Pages;
using PassMeta.DesktopApp.Ui.Models.ViewModels.Windows.MainWin;
using Splat;

namespace PassMeta.DesktopApp.Ui.Views.Windows.MainWin;

public class MainWindow : ReactiveWindow<MainWinModel>
{
    private bool _closingConfirmed;

    public MainWindow()
    {
        Opened += OnOpened;
        Closing += OnClosing;

        AvaloniaXamlLoader.Load(this);

#if DEBUG
        this.AttachDevTools();
#endif
    }

    private async void OnOpened(object? sender, EventArgs e)
    {
        var userContext = Locator.Current.Resolve<IUserContextProvider>();

        var vm = userContext.Current.UserId is null
            ? new AuthPageModel(ViewModel!)
            : new AccountPageModel(ViewModel!) as PageViewModel;
            //TODO : new StoragePageModel(ViewModel!) as PageViewModel;

        await vm.TryNavigateAsync();
    }

    private async void OnClosing(object? sender, CancelEventArgs e)
    {
        var pfcManager = Locator.Current.Resolve<IPassFileContextManager>();

        if (_closingConfirmed || pfcManager.Contexts.All(x => !x.AnyChanged))
        {
            return;
        }

        e.Cancel = true;
        var dialogService = Locator.Current.Resolve<IDialogService>();

        var confirm = await dialogService.ConfirmAsync(Common.Resources.APP__CONFIRM_ROLLBACK_ON_QUIT);
        if (!confirm.Ok) return;

        _closingConfirmed = true;
        Close();
    }
}