using System;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using PassMeta.DesktopApp.Common.Interfaces.Services;
using PassMeta.DesktopApp.Core.Utils;
using PassMeta.DesktopApp.Ui.ViewModels;
using Splat;

namespace PassMeta.DesktopApp.Ui.Views
{
    public class SettingsView : ReactiveUserControl<SettingsViewModel>
    {
        public static event Action? OnCultureChanged;

        public SettingsView()
        {
            AvaloniaXamlLoader.Load(this);
        }
        
        private async void SaveBtn_OnClick(object? sender, RoutedEventArgs e)
        {
            var context = (SettingsViewModel)DataContext!;
            
            var serverUrl = context.ServerUrl?.Trim() ?? "";
            var lang = context.Lang[context.CultureIndex][1];

            if (serverUrl.Length > 0 && (!serverUrl.StartsWith("https://") || serverUrl.Length < 11))
            {
#if !DEBUG
                Locator.Current.GetService<IDialogService>()!.ShowError(Core.Resources.SETTINGS__INCORRECT_API);
                return;
#endif
            }

            var oldConfig = AppConfig.Current;

            var result = await AppConfig.CreateAndSetCurrentAsync(serverUrl, lang);
            if (result.Failure) return;

            var current = result.Data;
            
            if (current.CultureCode != oldConfig.CultureCode)
            {
                OnCultureChanged?.Invoke();
            }
            
            Locator.Current.GetService<IDialogService>()!.ShowInfo(Core.Resources.SETTINGS__SAVE_SUCCESS);
        }
    }
}