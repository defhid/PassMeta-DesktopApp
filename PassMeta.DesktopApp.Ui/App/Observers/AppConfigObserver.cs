using System;
using System.Globalization;
using System.Threading;
using PassMeta.DesktopApp.Common;
using PassMeta.DesktopApp.Common.Abstractions.AppConfig;
using PassMeta.DesktopApp.Common.Abstractions.AppContext;
using PassMeta.DesktopApp.Common.Abstractions.Services.PassMetaServices;
using PassMeta.DesktopApp.Common.Abstractions.Utils.Logging;
using PassMeta.DesktopApp.Core;
using PassMeta.DesktopApp.Core.Services.Extensions;

namespace PassMeta.DesktopApp.Ui.App.Observers;

public class AppConfigObserver : IObserver<IAppConfig>
{
    private readonly IPassMetaInfoService _pmInfoService;
    private readonly IAppContextManager _appContextManager;
    private readonly ILogsWriter _logger;
    private IAppConfig? _prev;
        
    public AppConfigObserver(IPassMetaInfoService pmInfoService, IAppContextManager appContextManager, ILogsWriter logger)
    {
        _pmInfoService = pmInfoService;
        _appContextManager = appContextManager;
        _logger = logger;
    }

    public void OnCompleted()
    {
    }

    public void OnError(Exception error)
    {
    }

    public async void OnNext(IAppConfig value)
    {
        try
        {
            if (value.Culture != _prev?.Culture)
            {
                Resources.Culture = value.Culture;

                Thread.CurrentThread.CurrentCulture = Resources.Culture;
                Thread.CurrentThread.CurrentUICulture = Resources.Culture;
                CultureInfo.DefaultThreadCurrentCulture = Resources.Culture;
                CultureInfo.DefaultThreadCurrentUICulture = Resources.Culture;

                Avalonia.Threading.Dispatcher.UIThread.Post(() =>
                {
                    Thread.CurrentThread.CurrentCulture = Resources.Culture;
                    Thread.CurrentThread.CurrentUICulture = Resources.Culture;
                });
            }

            if (_prev is null)
            {
                _prev = value;
                return;
            }

            if (value.ServerUrl != _prev.ServerUrl)
            {
                using var loading = AppLoading.General.Begin();

                var result = await _pmInfoService.LoadAsync();
                if (result.Ok)
                {
                    await _appContextManager.RefreshFromAsync(result.Data!);
                }
            }

            
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Processing of app-config-changing event failed");
        }

        _prev = value;
    }
}