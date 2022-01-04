namespace PassMeta.DesktopApp.Ui.Views.Main
{
    using DesktopApp.Core.Utils;
    using DesktopApp.Ui.ViewModels;
    using DesktopApp.Ui.ViewModels.Base;
    using DesktopApp.Ui.ViewModels.Main;
    using DesktopApp.Ui.ViewModels.Storage;
    
    using System;
    using System.Reactive.Linq;
    using System.Threading.Tasks;
    
    using Avalonia;
    using Avalonia.Controls;
    using Avalonia.Controls.Primitives;
    using Avalonia.Interactivity;
    using Avalonia.Markup.Xaml;
    
    using AppContext = Core.Utils.AppContext;

    public class MainWindow : Window
    {
        public static MainWindow? Current { get; private set; }

        public static event Func<MainWindow, Task>? CurrentChanged;
        
        private bool _loaded;
        
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
            set => base.DataContext = value;
        }

        private void MainPaneOpenCloseBtn_OnClick(object? sender, RoutedEventArgs e)
        {
            DataContext.IsMainPaneOpened = ((ToggleButton)sender!).IsChecked is true;
        }

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
            await PassMetaApi.CheckConnectionAsync();

            DataContext.Router.CurrentViewModel!.OfType<ViewModelPage>()
                .FirstAsync()
                .Subscribe(vm => vm.RefreshAsync());
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
            
            PassMetaApi.OnlineChanged += DataContext.OnlineChanged;
        }
        
        private void OnClosing(object? sender, EventArgs e)
        {
            PassMetaApi.OnlineChanged -= DataContext.OnlineChanged;
        }
        
        private void SubscribeOnPageEvents()
        {
            ViewModelPage.OnNavigated += (page) =>
            {
                DataContext.ActiveMainPaneButtonIndex = page switch
                {
                    AuthViewModel => 0,
                    AccountViewModel => 0,
                    StorageViewModel => 1,
                    GeneratorViewModel => 2,
                    SettingsViewModel => 3,
                    _ => DataContext.ActiveMainPaneButtonIndex
                };
                DataContext.IsMainPaneOpened = false;
                DataContext.RightBarButtons = page.RightBarButtons;
            };
        }
    }
}