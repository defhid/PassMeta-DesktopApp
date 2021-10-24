using System;
using System.Reactive.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using PassMeta.DesktopApp.Core.Utils;
using PassMeta.DesktopApp.Ui.Services;
using PassMeta.DesktopApp.Ui.ViewModels;
using PassMeta.DesktopApp.Ui.ViewModels.Base;
using PassMeta.DesktopApp.Ui.ViewModels.Main;
using PassMeta.DesktopApp.Ui.ViewModels.Storage;

namespace PassMeta.DesktopApp.Ui.Views.Main
{
    public class MainWindow : Window
    {
        private bool _loaded;
        
        public MainWindow()
        {
            SubscribeOnPageEvents();
            Opened += OnOpened;
            
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
            if (AppConfig.Current.ServerVersion is null)
            {
                await AppConfig.Current.RefreshFromServerAsync();
            }

            DataContext.Router.CurrentViewModel!.OfType<ViewModelPage>()
                .FirstAsync()
                .Subscribe(vm => vm.RefreshAsync());
        }

        private async void OnOpened(object? sender, EventArgs e)
        {
            await DialogService.SetCurrentWindowAsync(this);

            if (!_loaded)
            {
                if (AppConfig.Current.User is null)
                    new AuthViewModel(DataContext).Navigate();
                else
                    new StorageViewModel(DataContext).Navigate();
            }

            _loaded = true;
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

            SettingsView.OnCultureChanged += App.Restart;
        }
    }
}