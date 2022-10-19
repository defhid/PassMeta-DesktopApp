namespace PassMeta.DesktopApp.Ui.ViewModels
{
    using DesktopApp.Common;
    using DesktopApp.Common.Constants;
    using DesktopApp.Core.Utils;
    using DesktopApp.Core;
    using DesktopApp.Ui.ViewModels.Base;
    
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Common.Abstractions.Services;
    using Common.Models.Settings;
    using ReactCommand = ReactiveUI.ReactiveCommand<System.Reactive.Unit, System.Reactive.Unit>;
    
    using ReactiveUI;
    using Views.Etc;
    using Views.Main;

    public class SettingsViewModel : PageViewModel
    {
        private readonly IDialogService _dialogService = EnvironmentContainer.Resolve<IDialogService>();
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
                () => new ApplicationInfoWin().ShowDialog(MainWindow.Current));

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
            ServerUrl = string.IsNullOrWhiteSpace(AppConfig.Current.ServerUrl)
                ? "https://"
                : AppConfig.Current.ServerUrl;

            SelectedCulture = Cultures.FirstOrDefault(cult => cult.Code == AppConfig.Current.CultureCode);

            HidePasswords = AppConfig.Current.HidePasswords;
            DevMode = AppConfig.Current.DevMode;
        }

        private async Task _SaveAsync()
        {
            using var preloader = MainWindow.Current!.StartPreloader();
            
            var serverUrl = ServerUrl?.Trim() ?? "";

            if (serverUrl.Length > 0 && (!serverUrl.StartsWith("https://") || serverUrl.Length < 11))
            {
#if !DEBUG
                _dialogService.ShowError(Common.Resources.SETTINGS__INCORRECT_API);
                return;
#endif
            }

            if (serverUrl != AppConfig.Current.ServerUrl && 
                PassFileManager.AnyCurrentChanged && 
                (await _dialogService.ConfirmAsync(Resources.SETTINGS__CONFIRM_SERVER_CHANGE)).Bad)
            {
                return;
            }

            var result = await AppConfig.CreateAndSetCurrentAsync(new AppConfigData
            {
                ServerUrl = serverUrl,
                CultureCode = SelectedCulture?.Code,
                HidePasswords = HidePasswords,
                DevMode = DevMode,
                DefaultPasswordLength = AppConfig.Current.DefaultPasswordLength
            });

            if (result.Ok)
                _dialogService.ShowInfo(Resources.SETTINGS__INFO_SAVE_SUCCESS);
            else
                _dialogService.ShowError(result.Message!);

            this.RaisePropertyChanged(nameof(ServerInfo));
        }
    }
}