using System;
using PassMeta.DesktopApp.Common.Abstractions.App;
using PassMeta.DesktopApp.Common.Abstractions.PassFileContext;
using PassMeta.DesktopApp.Common.Abstractions.Utils.Logging;
using PassMeta.DesktopApp.Common.Extensions;
using Splat;

namespace PassMeta.DesktopApp.Ui.App.Observers;

public class UserContextObserver : IObserver<IUserContext>
{
    private static IPassFileContextManager PfContextManager => Locator.Current.Resolve<IPassFileContextManager>();
    private static ILogsWriter LogsWriter => Locator.Current.Resolve<ILogsWriter>();

    public void OnNext(IUserContext value)
    {
        try
        {
            PfContextManager.Reload(value);
        }
        catch (Exception ex)
        {
            LogsWriter.Error(ex, "Processing of app-config-changing event failed");
        }
    }

    public void OnCompleted()
    {
    }

    public void OnError(Exception error)
    {
    }
}