using System;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using Avalonia.Threading;
using PassMeta.DesktopApp.Common.Abstractions.App;
using PassMeta.DesktopApp.Common.Abstractions.PassFileContext;
using PassMeta.DesktopApp.Common.Abstractions.Services;
using PassMeta.DesktopApp.Common.Abstractions.Utils.PassMetaClient;
using PassMeta.DesktopApp.Common.Extensions;
using PassMeta.DesktopApp.Common.Models.App;
using PassMeta.DesktopApp.Ui.Models.ViewModels.Base;
using PassMeta.DesktopApp.Ui.Models.ViewModels.Pages;
using PassMeta.DesktopApp.Ui.Models.ViewModels.Pages.StoragePage;
using PassMeta.DesktopApp.Ui.Models.ViewModels.Windows.MainWin;
using ReactiveUI;
using Splat;

namespace PassMeta.DesktopApp.Ui.Views.Windows;

public class MainWindow : ReactiveWindow<MainWinModel>
{
    private bool _closingConfirmed;

    public MainWindow()
    {
        Opened += OnOpened;
        Closing += OnClosing;

        AvaloniaXamlLoader.Load(this);

        this.WhenActivated(d =>
        {
            d(Locator.Current.Resolve<AppLoading>().General.ActiveObservable.Subscribe(HandleGeneralLoading));

        });
    }

    private async void RefreshBtn_OnClick(object? sender, RoutedEventArgs e)
    {
        using var preloader = Locator.Current.Resolve<AppLoading>().General.Begin();

        await Locator.Current.Resolve<IPassMetaClient>().CheckConnectionAsync();

        if (await ViewModel!.Router.CurrentViewModel is PageViewModel pvm)
        {
            await pvm.RefreshAsync();
        }
    }

    private async void OnOpened(object? sender, EventArgs e)
    {
        var userContext = Locator.Current.Resolve<IUserContextProvider>();

        var vm = userContext.Current.UserId is null
            ? new AuthPageModel(ViewModel!)
            : new StoragePageModel(ViewModel!) as PageViewModel;

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

    private void HandleGeneralLoading(bool isLoading)
    {
        Dispatcher.UIThread.InvokeAsync(() => ViewModel!.PreloaderEnabled = isLoading);
    }
}