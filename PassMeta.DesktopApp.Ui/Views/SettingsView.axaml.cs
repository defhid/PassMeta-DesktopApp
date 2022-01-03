namespace PassMeta.DesktopApp.Ui.Views
{
    using DesktopApp.Common.Interfaces.Services;
    using DesktopApp.Core.Utils;
    using DesktopApp.Ui.ViewModels;
    using DesktopApp.Ui.Views.Base;
    
    using Avalonia.Interactivity;
    using Avalonia.Markup.Xaml;
    using Splat;
    
    public class SettingsView : ViewPage<SettingsViewModel>
    {
        private readonly IDialogService _dialogService = Locator.Current.GetService<IDialogService>()!;

        public SettingsView()
        {
            AvaloniaXamlLoader.Load(this);
        }

        private async void SaveBtn_OnClick(object? sender, RoutedEventArgs e)
        {
            var context = DataContext!;
            
            var serverUrl = context.ServerUrl?.Trim() ?? "";
            var lang = context.Lang[context.CultureIndex][1];

            if (serverUrl.Length > 0 && (!serverUrl.StartsWith("https://") || serverUrl.Length < 11))
            {
#if !DEBUG
                _dialogService.ShowError(Common.Resources.SETTINGS__INCORRECT_API);
                return;
#endif
            }
            
            var result = await AppConfig.CreateAndSetCurrentAsync(serverUrl, lang);
            if (result.Bad) return;

            _dialogService.ShowInfo(Common.Resources.SETTINGS__SAVE_SUCCESS);
        }
    }
}