using System;
using PassMeta.DesktopApp.Common.Abstractions.Services.Logging;
using PassMeta.DesktopApp.Common.Abstractions.Utils.PassMetaClient;
using PassMeta.DesktopApp.Core;
using PassMeta.DesktopApp.Core.Services.Extensions;
using AppContext = PassMeta.DesktopApp.Core.AppContext;

namespace PassMeta.DesktopApp.Ui.App.Observers;

public class OnlineObserver : IObserver<bool>
{
    private readonly IPassMetaClient _passMetaClient;
    private readonly ILogService _logger;
    private bool _prev;

    public OnlineObserver(IPassMetaClient passMetaClient, ILogService logger)
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

    public async void OnNext(bool value)
    {
        try
        {
            if (value && value != _prev && AppContext.Current.ServerVersion is null)
            {
                await AppContext.RefreshCurrentAsync(AppConfig.Current, _passMetaClient);
            }
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Processing of online-changing event failed");
        }

        _prev = value;
    }
}