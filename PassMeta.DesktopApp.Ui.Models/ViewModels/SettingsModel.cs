using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;
using PassMeta.DesktopApp.Common;
using PassMeta.DesktopApp.Common.Abstractions.App;
using PassMeta.DesktopApp.Common.Abstractions.PassFileContext;
using PassMeta.DesktopApp.Common.Abstractions.Services;
using PassMeta.DesktopApp.Common.Constants;
using PassMeta.DesktopApp.Common.Extensions;
using PassMeta.DesktopApp.Common.Models.App;
using PassMeta.DesktopApp.Ui.Models.Base;
using PassMeta.DesktopApp.Ui.Models.Etc;
using ReactiveUI;
using Splat;
using ReactCommand = ReactiveUI.ReactiveCommand<System.Reactive.Unit, System.Reactive.Unit>;

namespace PassMeta.DesktopApp.Ui.Models;

public class SettingsViewModel : PageViewModel
{
    private readonly IDialogService _dialogService = Locator.Current.Resolve<IDialogService>();
    private readonly IAppConfigManager _appConfigManager = Locator.Current.Resolve<IAppConfigManager>();
    private readonly IAppContextProvider _appContextProvider = Locator.Current.Resolve<IAppContextProvider>();
    private readonly IPassFileContextProvider _pfContextProvider = Locator.Current.Resolve<IPassFileContextProvider>();
    private bool _devMode;

    public Interaction<ApplicationInfoViewModel, Unit> ShowInfo = new();

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

    public static string AppInfo => $"v{Locator.Current.Resolve<AppInfo>().Version}";
        
    public ReactCommand AppInfoCommand { get; }
        
    public ReactCommand SaveCommand { get; }

    public SettingsViewModel(IScreen hostScreen) : base(hostScreen)
    {
        FillFromAppConfig();

        AppInfoCommand = ReactiveCommand.CreateFromTask(
            async () => await ShowInfo.Handle(new ApplicationInfoViewModel()));

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
        using var preloader = Locator.Current.Resolve<AppLoading>().General.Begin();
            
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

    private bool IsValidServerUrl(string url) => url.StartsWith("https://") && url.Length > 11 || _devMode;
}