using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using PassMeta.DesktopApp.Common.Interfaces.Services;
using PassMeta.DesktopApp.Ui.Services;
using PassMeta.DesktopApp.Ui.ViewModels;
using ReactiveUI;
using Splat;

namespace PassMeta.DesktopApp.Ui.Views.Main
{
    public class MainWindow : Window
    {
        public MainWindow()
        {
            AvaloniaXamlLoader.Load(this);
#if DEBUG
            this.AttachDevTools();
#endif
        }

        private MainWindowViewModel _GetDataContext() => (MainWindowViewModel)DataContext!;

        private void _SetActiveMainPaneButton(int buttonIndex)
        {
            var context = _GetDataContext();
            context.ActiveMainPaneButtonIndex = buttonIndex;
            context.IsMainPaneOpened = false;
        }

        private void _NavigateTo(Func<MainWindowViewModel, IRoutableViewModel> vmBuilder)
        {
            var context = _GetDataContext();
            context.Router.Navigate.Execute(vmBuilder(context));
        }

        private void MainPaneOpenCloseBtn_OnClick(object? sender, RoutedEventArgs e)
        {
            var context = _GetDataContext();
            context.IsMainPaneOpened = ((ToggleButton)sender!).IsChecked ?? false;
        }

        private void AccountBtn_OnClick(object? sender, RoutedEventArgs e)
        {
            _SetActiveMainPaneButton(0);
            _NavigateTo(context => new AccountViewModel(context));
        }
        
        private void StorageBtn_OnClick(object? sender, RoutedEventArgs e)
        {
            _SetActiveMainPaneButton(1);
            _NavigateTo(context => new StorageViewModel(context));
        }
        
        private void GeneratorBtn_OnClick(object? sender, RoutedEventArgs e)
        {
            _SetActiveMainPaneButton(2);
            _NavigateTo(context => new GeneratorViewModel(context));
            Locator.Current.GetService<IDialogService>().ShowErrorAsync("My Error!!!!!!!!!!")
                .GetAwaiter().GetResult();
        }
        
        private void SettingsBtn_OnClick(object? sender, RoutedEventArgs e)
        {
            _SetActiveMainPaneButton(3);
            _NavigateTo(context => new SettingsViewModel(context));
        }

        private async void Window_OnOpened(object? sender, EventArgs e)
        {
            await DialogService.SetCurrentWindowAsync(this);
        }
    }
}