namespace PassMeta.DesktopApp.Ui.Views.Main
{
    using DesktopApp.Core;
    using DesktopApp.Core.Utils;
    using DesktopApp.Common.Interfaces.Services;
    
    using DesktopApp.Ui.Utils;
    using DesktopApp.Ui.ViewModels;
    using DesktopApp.Ui.ViewModels.Base;
    using DesktopApp.Ui.ViewModels.Main.MainWindow;
    using DesktopApp.Ui.ViewModels.Storage.Storage;
    using DesktopApp.Ui.ViewModels.Journal;
    using DesktopApp.Ui.ViewModels.Logs;
    
    using AppContext = Core.Utils.AppContext;
    
    using System;
    using System.ComponentModel;
    using System.Reactive.Linq;
    using System.Threading.Tasks;
    using Avalonia.Controls;
    using Avalonia.Interactivity;
    using Avalonia.Markup.Xaml;
    using Avalonia.Threading;
    using Base;

    public class MainWindow : WinView<MainWindowViewModel>
    {
        public static MainWindow? Current { get; private set; }

        public static event Func<MainWindow, Task>? CurrentChanged;

        public MainWindow()
        {
            Opened += OnOpened;
            Closing += OnClosing;

            AvaloniaXamlLoader.Load(this);
        }

        public Preloader StartPreloader() => new Preloader(DataContext!).Start();

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
            using var preloader = StartPreloader();
            
            await PassMetaApi.CheckConnectionAsync();

            await DataContext!.Router.CurrentViewModel!.OfType<PageViewModel>()
                .FirstAsync()
                .Select(vm => vm.RefreshAsync());
        }

        private async void OnOpened(object? sender, EventArgs e)
        {
            PageViewModel.Navigated += HandleNavigate;
            
            var firstLoad = Current is null;
            Current = this;
            
            if (CurrentChanged != null)
                await CurrentChanged(this);

            if (firstLoad)
            {
                await Dispatcher.UIThread.InvokeAsync(StartUp.LoadContextAndCheckSystemAsync, DispatcherPriority.Background);
            }

            InitNavigate();
            
            DataContext!.PreloaderEnabled = false;
        }
        
        private async void OnClosing(object? sender, CancelEventArgs e)
        {
            if (ReferenceEquals(Current, this) && PassFileManager.AnyCurrentChanged)
            {
                e.Cancel = true;
                var dialogService = EnvironmentContainer.Resolve<IDialogService>();

                var confirm = await dialogService.ConfirmAsync(Common.Resources.APP__CONFIRM_ROLLBACK_ON_QUIT);
                if (!confirm.Ok) return;

                var win = Current;
                Current = null;
                win.Close();
            }
            else
            {
                PageViewModel.Navigated -= HandleNavigate;
            }
        }

        private void InitNavigate()
        {
            if (AppContext.Current.User is null)
                new AuthViewModel(DataContext!).TryNavigate();
            else
                new StorageViewModel(DataContext!).TryNavigate();
        }

        private void HandleNavigate(object? sender, EventArgs e)
        {
            var mainPaneButtons = DataContext!.MainPane.Buttons;
            mainPaneButtons.CurrentActive = sender switch
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
            DataContext.RightBarButtons = (sender as PageViewModel)?.RightBarButtons;
        }
    }
}