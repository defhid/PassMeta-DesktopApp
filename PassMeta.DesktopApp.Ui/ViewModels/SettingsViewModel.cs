using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;

using ReactiveUI;
using ReactCommand = ReactiveUI.ReactiveCommand<System.Reactive.Unit, System.Reactive.Unit>;
    
using PassMeta.DesktopApp.Common;
using PassMeta.DesktopApp.Common.Abstractions.AppConfig;
using PassMeta.DesktopApp.Common.Abstractions.AppContext;
using PassMeta.DesktopApp.Common.Abstractions.PassFileContext;
using PassMeta.DesktopApp.Common.Abstractions.Services;
using PassMeta.DesktopApp.Common.Constants;
using PassMeta.DesktopApp.Core.Extensions;
using PassMeta.DesktopApp.Ui.App;
using PassMeta.DesktopApp.Ui.Views.Etc;
using PassMeta.DesktopApp.Ui.ViewModels.Base;
using Splat;

namespace PassMeta.DesktopApp.Ui.ViewModels;

public class SettingsViewModel : PageViewModel
{
    private readonly IDialogService _dialogService = Locator.Current.Resolve<IDialogService>();
    private readonly IAppConfigManager _appConfigManager = Locator.Current.Resolve<IAppConfigManager>();
    private readonly IAppContextProvider _appContextProvider = Locator.Current.Resolve<IAppContextProvider>();
    private readonly IPassFileContextProvider _pfContextProvider = Locator.Current.Resolve<IPassFileContextProvider>();
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

    public IObservable<string> ServerInfo => _appContextProvider.CurrentObservable
        .Select(x => x.ServerVersion is null 
            ? string.Empty 
            : $"v{x.ServerVersion}, #{x.ServerId ?? "?"}");  // TODO: dispose?

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
        var appConfig = _appConfigManager.Current;

        ServerUrl = string.IsNullOrWhiteSpace(appConfig.ServerUrl)
            ? "https://"
            : appConfig.ServerUrl;

        SelectedCulture = Cultures.FirstOrDefault(cult => cult == appConfig.Culture);

        HidePasswords = appConfig.HidePasswords;
        DevMode = appConfig.DevMode;
    }

    private async Task _SaveAsync()
    {
        using var preloader = AppLoading.General.Begin();
            
        var serverUrl = ServerUrl?.Trim() ?? "";

        if (serverUrl.Length > 0 && !IsValidServerUrl(serverUrl))
        {
            _dialogService.ShowError(Resources.SETTINGS__INCORRECT_API);
            return;
        }

        if (serverUrl != _appConfigManager.Current.ServerUrl && 
            _pfContextProvider.Contexts.Any(x => x.AnyChanged))
        {
            var confirm = await _dialogService.ConfirmAsync(Resources.SETTINGS__CONFIRM_SERVER_CHANGE);
            if (confirm.Bad) return;
        }

        var result = await _appConfigManager.ApplyAsync(appConfig =>
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

    private static bool IsValidServerUrl(string url) => url.StartsWith("https://") && url.Length > 11;
}