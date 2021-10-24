using System;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using PassMeta.DesktopApp.Common.Interfaces.Services;
using PassMeta.DesktopApp.Core.Utils;
using PassMeta.DesktopApp.Ui.ViewModels;
using PassMeta.DesktopApp.Ui.Views.Base;
using Splat;

namespace PassMeta.DesktopApp.Ui.Views
{
    public class SettingsView : ViewPage<SettingsViewModel>
    {
        public static event Action? OnCultureChanged;

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
                Locator.Current.GetService<IDialogService>()!.ShowError(Common.Resources.SETTINGS__INCORRECT_API);
                return;
#endif
            }

            var oldConfig = AppConfig.Current;

            var result = await AppConfig.CreateAndSetCurrentAsync(serverUrl, lang);
            if (result.Bad) return;

            var current = result.Data!;
            
            if (current.CultureCode != oldConfig.CultureCode)
            {
                OnCultureChanged?.Invoke();
            }
            
            Locator.Current.GetService<IDialogService>()!.ShowInfo(Common.Resources.SETTINGS__SAVE_SUCCESS);
        }
    }
}