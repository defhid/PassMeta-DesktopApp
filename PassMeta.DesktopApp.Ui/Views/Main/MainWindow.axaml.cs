namespace PassMeta.DesktopApp.Ui.Views.Main
{
    using DesktopApp.Core.Utils;
    using DesktopApp.Ui.Utils;
    using DesktopApp.Ui.ViewModels;
    using DesktopApp.Ui.ViewModels.Base;
    using DesktopApp.Ui.ViewModels.Main.MainWindow;
    using DesktopApp.Ui.ViewModels.Storage.Storage;
    
    using AppContext = Core.Utils.AppContext;
    
    using System;
    using System.ComponentModel;
    using System.Reactive.Linq;
    using System.Threading.Tasks;
    
    using Avalonia;
    using Avalonia.Controls;
    using Avalonia.Interactivity;
    using Avalonia.Markup.Xaml;
    using Common.Interfaces.Services;
    using Splat;

    public class MainWindow : Window
    {
        private bool _loaded;
        
        public static MainWindow? Current { get; private set; }

        public static event Func<MainWindow, Task>? CurrentChanged;

        public MainWindow()
        {
            SubscribeOnPageEvents();
            
            Opened += OnOpened;
            Closing += OnClosing;

            AvaloniaXamlLoader.Load(this);
#if DEBUG
            this.AttachDevTools();
#endif
        }

        public new MainWindowViewModel DataContext
        {
            get => (MainWindowViewModel)base.DataContext!;
            init => base.DataContext = value;
        }
        
        public Preloader StartPreloader() => new Preloader(DataContext).Start();

        private void AccountBtn_OnClick(object? sender, RoutedEventArgs e) 
            => new AccountViewModel(DataContext).Navigate();

        private void StorageBtn_OnClick(object? sender, RoutedEventArgs e)
            => new StorageViewModel(DataContext).Navigate();

        private void GeneratorBtn_OnClick(object? sender, RoutedEventArgs e)
            => new GeneratorViewModel(DataContext).Navigate();

        private void SettingsBtn_OnClick(object? sender, RoutedEventArgs e)
            => new SettingsViewModel(DataContext).Navigate();

        private async void RefreshBtn_OnClick(object? sender, RoutedEventArgs e)
        {
            using var preloader = StartPreloader();
            
            await PassMetaApi.CheckConnectionAsync();

            await DataContext.Router.CurrentViewModel!.OfType<ViewModelPage>()
                .FirstAsync()
                .Select(vm => vm.RefreshAsync());
        }

        private async void OnOpened(object? sender, EventArgs e)
        {
            Current = this;
            
            if (CurrentChanged != null)
                await CurrentChanged(this);

            if (!_loaded)
            {
                if (AppContext.Current.User is null)
                    new AuthViewModel(DataContext).Navigate();
                else
                    new StorageViewModel(DataContext).Navigate();
            }

            _loaded = true;
            
            PassMetaApi.OnlineChanged += DataContext.Mode.OnChanged;
        }
        
        private async void OnClosing(object? sender, CancelEventArgs e)
        {
            if (ReferenceEquals(Current, this) && PassFileManager.AnyCurrentChanged)
            {
                e.Cancel = true;
                var dialogService = Locator.Current.GetService<IDialogService>()!;
                var confirm = await dialogService.ConfirmAsync(Common.Resources.APP__CONFIRM_ROLLBACK_ON_QUIT);
                if (confirm.Ok)
                {
                    var win = Current;
                    Current = null;
                    win.Close();
                }
            }
            
            PassMetaApi.OnlineChanged -= DataContext.Mode.OnChanged;
        }
        
        private void SubscribeOnPageEvents()
        {
            ViewModelPage.OnNavigated += (page) =>
            {
                var mainPaneButtons = DataContext.MainPane.Buttons;
                mainPaneButtons.CurrentActive = page switch
                {
                    AuthViewModel => mainPaneButtons.Account,
                    AccountViewModel => mainPaneButtons.Account,
                    StorageViewModel => mainPaneButtons.Storage,
                    GeneratorViewModel => mainPaneButtons.Generator,
                    SettingsViewModel => mainPaneButtons.Settings,
                    _ => mainPaneButtons.CurrentActive
                };
                DataContext.MainPane.IsOpened = false;
                DataContext.RightBarButtons = page.RightBarButtons;
            };
        }
    }
}