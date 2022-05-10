namespace PassMeta.DesktopApp.Ui.ViewModels
{
    using DesktopApp.Common;
    using DesktopApp.Common.Constants;
    using DesktopApp.Common.Interfaces.Services;
    using DesktopApp.Core.Utils;
    using DesktopApp.Core;
    using DesktopApp.Ui.ViewModels.Base;
    
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Avalonia.Controls;
    
    using ReactCommand = ReactiveUI.ReactiveCommand<System.Reactive.Unit, System.Reactive.Unit>;
    
    using ReactiveUI;
    using Views.Main;

    public class SettingsViewModel : PageViewModel
    {
        private readonly IDialogService _dialogService = EnvironmentContainer.Resolve<IDialogService>();

        public override ContentControl[] RightBarButtons => new ContentControl[]
        {
            new Button
            {
                Content = "\uE74E",
                Command = ReactiveCommand.CreateFromTask(_SaveAsync),
                [ToolTip.TipProperty] = Resources.SETTINGS__RIGHT_BAR_TOOLTIP__SAVE
            }
        };

        public IReadOnlyList<AppCulture> Cultures => AppCulture.All;

        public string? ServerUrl { get; set; }

        public AppCulture? SelectedCulture { get; set; }

        public bool HidePasswords { get; set; }

        public SettingsViewModel(IScreen hostScreen) : base(hostScreen)
        {
            FillFromAppConfig();
        }

        public override Task RefreshAsync()
        {
            FillFromAppConfig();

            this.RaisePropertyChanged(nameof(ServerUrl));
            this.RaisePropertyChanged(nameof(SelectedCulture));

            return Task.CompletedTask;
        }

        private void FillFromAppConfig()
        {
            ServerUrl = string.IsNullOrWhiteSpace(AppConfig.Current.ServerUrl)
                ? "https://"
                : AppConfig.Current.ServerUrl;

            SelectedCulture = Cultures.FirstOrDefault(cult => cult.Code == AppConfig.Current.CultureCode);

            HidePasswords = AppConfig.Current.HidePasswords;
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
            
            var result = await AppConfig.CreateAndSetCurrentAsync(serverUrl, SelectedCulture, HidePasswords);
            if (result.Ok)
                _dialogService.ShowInfo(Resources.SETTINGS__INFO_SAVE_SUCCESS);
            else
                _dialogService.ShowError(result.Message!);
        }
    }
}