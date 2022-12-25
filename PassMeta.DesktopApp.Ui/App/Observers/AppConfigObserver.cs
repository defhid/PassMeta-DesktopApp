using System;
using System.Globalization;
using System.Threading;
using PassMeta.DesktopApp.Common.Abstractions.Services.Logging;
using PassMeta.DesktopApp.Common;
using PassMeta.DesktopApp.Common.Abstractions;
using PassMeta.DesktopApp.Common.Abstractions.Utils.PassMetaClient;

using PassMeta.DesktopApp.Core.Utils;
using PassMeta.DesktopApp.Core.Services.Extensions;
using AppContext = PassMeta.DesktopApp.Core.AppContext;

namespace PassMeta.DesktopApp.Ui.App.Observers;

public class AppConfigObserver : IObserver<IAppConfig>
{
    private readonly IPassMetaClient _passMetaClient;
    private readonly ILogService _logger;
    private IAppConfig? _prev;
        
    public AppConfigObserver(IPassMetaClient passMetaClient, ILogService logger)
    {
        _passMetaClient = passMetaClient;
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
            Resources.Culture = value.Culture;

            Thread.CurrentThread.CurrentCulture = Resources.Culture;
            Thread.CurrentThread.CurrentUICulture = Resources.Culture;
            CultureInfo.DefaultThreadCurrentCulture = Resources.Culture;
            CultureInfo.DefaultThreadCurrentUICulture = Resources.Culture;

            if (_prev is null)
            {
                _prev = value;
                return;
            }

            if (value.ServerUrl != _prev.ServerUrl)
            {
                await _passMetaClient.CheckConnectionAsync();

                await AppContext.RefreshCurrentAsync(value, _passMetaClient);

                await PassFileManager.ReloadAsync(false);
            }

            if (value.Culture != _prev.Culture)
            {
                App.ReopenMainWindow();
            }
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Processing of app-config-changing event failed");
        }

        _prev = value;
    }
}