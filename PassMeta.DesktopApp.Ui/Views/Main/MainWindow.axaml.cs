using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reactive.Linq;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using ReactiveUI;

using PassMeta.DesktopApp.Common.Abstractions.Services;
using PassMeta.DesktopApp.Common.Abstractions.Utils.PassMetaClient;
using PassMeta.DesktopApp.Core;
using PassMeta.DesktopApp.Core.Utils;
using AppContext = PassMeta.DesktopApp.Core.AppContext;

using PassMeta.DesktopApp.Ui.Views.Base;
using PassMeta.DesktopApp.Ui.ViewModels;
using PassMeta.DesktopApp.Ui.ViewModels.Base;
using PassMeta.DesktopApp.Ui.ViewModels.Main.MainWindow;
using PassMeta.DesktopApp.Ui.ViewModels.Storage.Storage;
using PassMeta.DesktopApp.Ui.ViewModels.Journal;
using PassMeta.DesktopApp.Ui.ViewModels.Logs;

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
        => MenuBtnClick(sender, () => new AccountViewModel(DataContext!).TryNavigate());

    private void StorageBtn_OnClick(object? sender, RoutedEventArgs e)
        => MenuBtnClick(sender, () => new StorageViewModel(DataContext!).TryNavigate());

    private void GeneratorBtn_OnClick(object? sender, RoutedEventArgs e)
        => MenuBtnClick(sender, () => new GeneratorViewModel(DataContext!).TryNavigate());
        
    private void JournalBtn_OnClick(object? sender, RoutedEventArgs e)
        => MenuBtnClick(sender, () => new JournalViewModel(DataContext!).TryNavigate());

    private void LogsBtn_OnClick(object? sender, RoutedEventArgs e)
        => MenuBtnClick(sender, () => new LogsViewModel(DataContext!).TryNavigate());

    private void SettingsBtn_OnClick(object? sender, RoutedEventArgs e)
        => MenuBtnClick(sender, () => new SettingsViewModel(DataContext!).TryNavigate());

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

        await EnvironmentContainer.Resolve<IPassMetaClient>().CheckConnectionAsync();

        await DataContext!.Router.CurrentViewModel!.OfType<PageViewModel>()
            .FirstOrDefaultAsync()
            .Select(vm => vm?.RefreshAsync());
    }

    private void OnOpened(object? sender, EventArgs e)
    {
        _disposables.Add(
            AppLoading.General.CurrentObservable.Subscribe(HandleGeneralLoading));

        _disposables.Add(
            DataContext!.Router.CurrentViewModel.Subscribe(HandleNavigate));

        if (AppContext.Current.User is null)
            new AuthViewModel(DataContext!).TryNavigate();
        else
            new StorageViewModel(DataContext!).TryNavigate();
            
        DataContext!.PreloaderEnabled = false;
    }

    private async void OnClosing(object? sender, CancelEventArgs e)
    {
        if (_closingConfirmed || !PassFileManager.AnyCurrentChanged)
        {
            foreach (var disposable in _disposables)
            {
                disposable.Dispose();
            }
            return;
        }

        e.Cancel = true;
        var dialogService = EnvironmentContainer.Resolve<IDialogService>();

        var confirm = await dialogService.ConfirmAsync(Common.Resources.APP__CONFIRM_ROLLBACK_ON_QUIT);
        if (!confirm.Ok) return;

        _closingConfirmed = true;
        Close();
    }

    private void HandleNavigate(IRoutableViewModel? viewModel)
    {
        var mainPaneButtons = DataContext!.MainPane.Buttons;
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
        DataContext.MainPane.IsOpened = false;
        DataContext.RightBarButtons = (viewModel as PageViewModel)?.RightBarButtons;
    }

    private void HandleGeneralLoading(bool isLoading)
    {
        Dispatcher.UIThread.InvokeAsync(() => DataContext!.PreloaderEnabled = isLoading);
    }
}