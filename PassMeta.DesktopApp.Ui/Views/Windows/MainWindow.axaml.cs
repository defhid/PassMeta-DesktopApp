using System;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;
using Avalonia.Controls;
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
using PassMeta.DesktopApp.Ui.Models.ViewModels.Pages.JournalPage;
using PassMeta.DesktopApp.Ui.Models.ViewModels.Pages.LogsPage;
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

            d(ViewModel!.Router.CurrentViewModel.Subscribe(HandleNavigate));
        });
    }

    private static void MenuBtnClick(object? sender, Action action)
    {
        var btn = (Button)sender!;
        if (!btn.Classes.Contains("active"))
        {
            action();
        }
    }

    private async void RefreshBtn_OnClick(object? sender, RoutedEventArgs e)
    {
        using var preloader = Locator.Current.Resolve<AppLoading>().General.Begin();

        await Locator.Current.Resolve<IPassMetaClient>().CheckConnectionAsync();

        await ViewModel!.Router.CurrentViewModel!.OfType<PageViewModel>()
            .FirstOrDefaultAsync()
            .Select(vm => vm?.RefreshAsync());
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

    private void HandleNavigate(IRoutableViewModel? viewModel)
    {
        var mainPaneButtons = ViewModel!.MainPane.Buttons;
        mainPaneButtons.CurrentActive = viewModel switch
        {
            AuthPageModel => mainPaneButtons.Account,
            AccountPageModel => mainPaneButtons.Account,
            StoragePageModel => mainPaneButtons.Storage,
            GeneratorPageModel => mainPaneButtons.Generator,
            JournalPageModel => mainPaneButtons.Journal,
            LogsPageModel => mainPaneButtons.Logs,
            SettingsPageModel => mainPaneButtons.Settings,
            _ => mainPaneButtons.CurrentActive
        };
        ViewModel.MainPane.IsOpened = false;
    }

    private void HandleGeneralLoading(bool isLoading)
    {
        Dispatcher.UIThread.InvokeAsync(() => ViewModel!.PreloaderEnabled = isLoading);
    }
}