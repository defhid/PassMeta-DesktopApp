using System;
using PassMeta.DesktopApp.Common.Abstractions.Services.Logging;
using PassMeta.DesktopApp.Common.Abstractions;
using PassMeta.DesktopApp.Core.Utils;
using PassMeta.DesktopApp.Core.Services.Extensions;

namespace PassMeta.DesktopApp.Ui.App.Observers;

public class AppContextObserver : IObserver<IAppContext>
{
    private readonly ILogService _logger;
    private IAppContext? _prev;

    public AppContextObserver(ILogService logger)
    {
        _logger = logger;
    }

    public void OnCompleted()
    {
    }

    public void OnError(Exception error)
    {
    }

    public async void OnNext(IAppContext value)
    {
        try
        {
            if (value.User?.Id != _prev?.User?.Id)
            {
                await PassFileManager.ReloadAsync(true);
            }
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Processing of app-context-changing event failed");
        }

        _prev = value;
    }
}