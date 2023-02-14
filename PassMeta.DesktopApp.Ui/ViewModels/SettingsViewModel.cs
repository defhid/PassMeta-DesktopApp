using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using ReactiveUI;
using ReactCommand = ReactiveUI.ReactiveCommand<System.Reactive.Unit, System.Reactive.Unit>;
    
using PassMeta.DesktopApp.Common;
using PassMeta.DesktopApp.Common.Abstractions.Services;
using PassMeta.DesktopApp.Common.Constants;
using PassMeta.DesktopApp.Core.Utils;
using PassMeta.DesktopApp.Core;
using PassMeta.DesktopApp.Ui.App;
using PassMeta.DesktopApp.Ui.Views.Etc;
using PassMeta.DesktopApp.Ui.ViewModels.Base;

namespace PassMeta.DesktopApp.Ui.ViewModels
{
    public class SettingsViewModel : PageViewModel
    {
        private readonly IDialogService _dialogService = Locator.Current.Resolve<IDialogService>();
        private bool _devMode;

        public IReadOnlyList<AppCulture> Cultures => AppCulture.All;

        public string? ServerUrl { get; set; }

        public AppCulture? SelectedCulture { get; set; }

        public bool HidePasswords { get; set; }

        public bool DevMode
        {
            get => _devMode;
            set
            {
                _devMode = value;
                this.RaisePropertyChanged(nameof(DevModeOpacity));
            }
        }

        public float DevModeOpacity => DevMode ? 1f : 0.5f;

        public static string ServerInfo => AppContext.Current.ServerVersion is null
            ? string.Empty
            : $"v{AppContext.Current.ServerVersion}, #{AppContext.Current.ServerId ?? "?"}";

        public static string AppInfo => $"v{Core.AppInfo.Version}";
        
        public ReactCommand AppInfoCommand { get; }
        
        public ReactCommand SaveCommand { get; }

        public SettingsViewModel(IScreen hostScreen) : base(hostScreen)
        {
            FillFromAppConfig();

            AppInfoCommand = ReactiveCommand.CreateFromTask(
                () => new ApplicationInfoWin().ShowDialog(App.App.MainWindow));

            SaveCommand = ReactiveCommand.CreateFromTask(_SaveAsync);
        }

        public override Task RefreshAsync()
        {
            FillFromAppConfig();

            this.RaisePropertyChanged(nameof(ServerUrl));
            this.RaisePropertyChanged(nameof(SelectedCulture));
            this.RaisePropertyChanged(nameof(HidePasswords));
            this.RaisePropertyChanged(nameof(ServerInfo));

            return Task.CompletedTask;
        }

        private void FillFromAppConfig()
        {
            ServerUrl = string.IsNullOrWhiteSpace(AppPaths.Current.ServerUrl)
                ? "https://"
                : AppPaths.Current.ServerUrl;

            SelectedCulture = Cultures.FirstOrDefault(cult => cult == AppPaths.Current.Culture);

            HidePasswords = AppPaths.Current.HidePasswords;
            DevMode = AppPaths.Current.DevMode;
        }

        private async Task _SaveAsync()
        {
            using var preloader = AppLoading.General.Begin();
            
            var serverUrl = ServerUrl?.Trim() ?? "";

            if (serverUrl.Length > 0 && (!serverUrl.StartsWith("https://") || serverUrl.Length < 11))
            {
#if !DEBUG
                _dialogService.ShowError(Common.Resources.SETTINGS__INCORRECT_API);
                return;
#endif
            }

            if (serverUrl != AppPaths.Current.ServerUrl && 
                PassFileManager.AnyCurrentChanged && 
                (await _dialogService.ConfirmAsync(Resources.SETTINGS__CONFIRM_SERVER_CHANGE)).Bad)
            {
                return;
            }

            var result = await AppPaths.ApplyAsync(appConfig =>
            {
                appConfig.ServerUrl = serverUrl;
                appConfig.Culture = SelectedCulture ?? appConfig.Culture;
                appConfig.HidePasswords = HidePasswords;
                appConfig.DevMode = DevMode;
            });

            if (result.Ok)
                _dialogService.ShowInfo(Resources.SETTINGS__INFO_SAVE_SUCCESS);
            else
                _dialogService.ShowError(result.Message!);

            this.RaisePropertyChanged(nameof(ServerInfo));
        }
    }
}