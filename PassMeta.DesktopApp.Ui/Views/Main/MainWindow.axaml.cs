using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using PassMeta.DesktopApp.Common.Abstractions.AppContext;
using PassMeta.DesktopApp.Common.Abstractions.PassFileContext;
using ReactiveUI;
using PassMeta.DesktopApp.Common.Abstractions.Services;
using PassMeta.DesktopApp.Common.Abstractions.Utils.PassMetaClient;
using PassMeta.DesktopApp.Core.Extensions;
using PassMeta.DesktopApp.Ui.App;
using PassMeta.DesktopApp.Ui.Views.Base;
using PassMeta.DesktopApp.Ui.ViewModels;
using PassMeta.DesktopApp.Ui.ViewModels.Base;
using PassMeta.DesktopApp.Ui.ViewModels.Main.MainWindow;
using PassMeta.DesktopApp.Ui.ViewModels.Storage.Storage;
using PassMeta.DesktopApp.Ui.ViewModels.Journal;
using PassMeta.DesktopApp.Ui.ViewModels.Logs;
using Splat;

namespace PassMeta.DesktopApp.Ui.Views.Main;

public class MainWindow : WinView<MainWindowViewModel>
{
    private readonly List<IDisposable> _disposables = new();
    private bool _closingConfirmed;

    public MainWindow()
    {
        Opened += OnOpened;
        Closing += OnClosing;

        AvaloniaXamlLoader.Load(this);
    }

    private void AccountBtn_OnClick(object? sender, RoutedEventArgs e)
        => MenuBtnClick(sender, () => new AccountViewModel(ViewModel!).TryNavigate());

    private void StorageBtn_OnClick(object? sender, RoutedEventArgs e)
        => MenuBtnClick(sender, () => new StorageViewModel(ViewModel!).TryNavigate());

    private void GeneratorBtn_OnClick(object? sender, RoutedEventArgs e)
        => MenuBtnClick(sender, () => new GeneratorViewModel(ViewModel!).TryNavigate());

    private void JournalBtn_OnClick(object? sender, RoutedEventArgs e)
        => MenuBtnClick(sender, () => new JournalViewModel(ViewModel!).TryNavigate());

    private void LogsBtn_OnClick(object? sender, RoutedEventArgs e)
        => MenuBtnClick(sender, () => new LogsViewModel(ViewModel!).TryNavigate());

    private void SettingsBtn_OnClick(object? sender, RoutedEventArgs e)
        => MenuBtnClick(sender, () => new SettingsViewModel(ViewModel!).TryNavigate());

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
        using var preloader = AppLoading.General.Begin();

        await Locator.Current.Resolve<IPassMetaClient>().CheckConnectionAsync();

        await ViewModel!.Router.CurrentViewModel!.OfType<PageViewModel>()
            .FirstOrDefaultAsync()
            .Select(vm => vm?.RefreshAsync());
    }

    private void OnOpened(object? sender, EventArgs e)
    {
        _disposables.Add(
            AppLoading.General.ActiveObservable.Subscribe(HandleGeneralLoading));

        _disposables.Add(
            ViewModel!.Router.CurrentViewModel.Subscribe(HandleNavigate));

        var userContext = Locator.Current.Resolve<IUserContextProvider>();

        if (userContext.Current.UserId is null)
            new AuthViewModel(ViewModel!).TryNavigate();
        else
            new StorageViewModel(ViewModel!).TryNavigate();

        ViewModel!.PreloaderEnabled = false;
    }

    private async void OnClosing(object? sender, CancelEventArgs e)
    {
        var pfcManager = Locator.Current.Resolve<IPassFileContextManager>();
        
        if (_closingConfirmed || pfcManager.Contexts.All(x => !x.AnyChanged))
        {
            foreach (var disposable in _disposables)
            {
                disposable.Dispose();
            }

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
            AuthViewModel => mainPaneButtons.Account,
            AccountViewModel => mainPaneButtons.Account,
            StorageViewModel => mainPaneButtons.Storage,
            GeneratorViewModel => mainPaneButtons.Generator,
            JournalViewModel => mainPaneButtons.Journal,
            LogsViewModel => mainPaneButtons.Logs,
            SettingsViewModel => mainPaneButtons.Settings,
            _ => mainPaneButtons.CurrentActive
        };
        ViewModel.MainPane.IsOpened = false;
        ViewModel.RightBarButtons = (viewModel as PageViewModel)?.RightBarButtons;
    }

    private void HandleGeneralLoading(bool isLoading)
    {
        Dispatcher.UIThread.InvokeAsync(() => ViewModel!.PreloaderEnabled = isLoading);
    }
}