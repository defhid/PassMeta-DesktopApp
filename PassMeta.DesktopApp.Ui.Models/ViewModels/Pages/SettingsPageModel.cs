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
using PassMeta.DesktopApp.Ui.Models.ViewModels.Base;
using PassMeta.DesktopApp.Ui.Models.ViewModels.Windows;
using ReactiveUI;
using Splat;
using ReactCommand = ReactiveUI.ReactiveCommand<System.Reactive.Unit, System.Reactive.Unit>;

namespace PassMeta.DesktopApp.Ui.Models.ViewModels.Pages;

/// <summary>
/// Settings page ViewModel.
/// </summary>
public class SettingsPageModel : PageViewModel
{
    private readonly IDialogService _dialogService = Locator.Current.Resolve<IDialogService>();
    private readonly IAppConfigManager _appConfigManager = Locator.Current.Resolve<IAppConfigManager>();
    private readonly IAppContextProvider _appContextProvider = Locator.Current.Resolve<IAppContextProvider>();
    private readonly IPassFileContextProvider _pfContextProvider = Locator.Current.Resolve<IPassFileContextProvider>();
    private bool _devMode;

    /// <summary></summary>
    public readonly Interaction<AppInfoWinModel, Unit> ShowInfo = new();

    /// <summary></summary>
    public SettingsPageModel(IScreen hostScreen) : base(hostScreen)
    {
        FillFromAppConfig();

        DevModeOpacity = this.WhenAnyValue(x => x.DevMode)
            .Select(devMode => devMode ? 1f : 0.5f);

        AppInfoCommand = ReactiveCommand.CreateFromTask(
            async () => await ShowInfo.Handle(new AppInfoWinModel()));

        SaveCommand = ReactiveCommand.CreateFromTask(SaveAsync);
    }

    #region preview

    /// <summary></summary>
    [Obsolete("PREVIEW constructor")]
    public SettingsPageModel() : this(null!)
    {
    }

    #endregion

    /// <summary></summary>
    public IReadOnlyList<AppCulture> Cultures => AppCulture.All;

    /// <summary></summary>
    public string? ServerUrl { get; set; }

    /// <summary></summary>
    public AppCulture? SelectedCulture { get; set; }

    /// <summary></summary>
    public bool HidePasswords { get; set; }

    /// <summary></summary>
    public bool DevMode
    {
        get => _devMode;
        set => this.RaiseAndSetIfChanged(ref _devMode, value);
    }

    /// <summary></summary>
    public IObservable<float> DevModeOpacity { get; }

    /// <summary></summary>
    public IObservable<string> ServerInfo => _appContextProvider.CurrentObservable
        .Select(x => x.ServerVersion is null
            ? string.Empty
            : $"v{x.ServerVersion}, #{x.ServerId ?? "?"}");

    /// <summary></summary>
    public static string AppInfo => $"v{Locator.Current.Resolve<AppInfo>().Version}";

    /// <summary></summary>
    public ReactCommand AppInfoCommand { get; }

    /// <summary></summary>
    public ReactCommand SaveCommand { get; }

    /// <inheritdoc />
    public override ValueTask RefreshAsync()
    {
        FillFromAppConfig();
        return ValueTask.CompletedTask;
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
        
        this.RaisePropertyChanged(nameof(ServerUrl));
        this.RaisePropertyChanged(nameof(SelectedCulture));
        this.RaisePropertyChanged(nameof(HidePasswords));
        this.RaisePropertyChanged(nameof(ServerInfo));
    }

    private async Task SaveAsync()
    {
        using var preloader = Locator.Current.Resolve<AppLoading>().General.Begin();

        var serverUrl = ServerUrl?.Trim() ?? "";
        var serverUrlChanged = serverUrl != _appConfigManager.Current.ServerUrl;

        if (serverUrlChanged)
        {
            if (serverUrl.Length > 0 && !IsValidServerUrl(serverUrl))
            {
                _dialogService.ShowError(Resources.SETTINGS__INCORRECT_API);
                return;
            }

            if (_pfContextProvider.Contexts.Any(x => x.AnyChanged))
            {
                var confirm = await _dialogService.ConfirmAsync(Resources.SETTINGS__CONFIRM_SERVER_CHANGE);
                if (confirm.Bad) return;
            }
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
    }

    private bool IsValidServerUrl(string url) => url.StartsWith("https://") && url.Length > 11 || _devMode;
}