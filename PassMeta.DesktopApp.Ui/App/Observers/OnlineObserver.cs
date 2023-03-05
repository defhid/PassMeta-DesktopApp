using System;
using PassMeta.DesktopApp.Common.Abstractions.App;
using PassMeta.DesktopApp.Common.Abstractions.Services.PassMetaServices;
using PassMeta.DesktopApp.Common.Abstractions.Utils.Logging;
using PassMeta.DesktopApp.Common.Extensions;
using PassMeta.DesktopApp.Common.Models.App;
using PassMeta.DesktopApp.Core;
using PassMeta.DesktopApp.Core.Extensions;
using PassMeta.DesktopApp.Ui.Models;
using Splat;

namespace PassMeta.DesktopApp.Ui.App.Observers;

public class OnlineObserver : IObserver<bool>
{
    private static IAppContextManager AppContextManager => Locator.Current.Resolve<IAppContextManager>();
    private static IPassMetaInfoService PmInfoService => Locator.Current.Resolve<IPassMetaInfoService>();
    private static ILogsWriter LogsWriter => Locator.Current.Resolve<ILogsWriter>();

    public async void OnNext(bool value)
    {
        try
        {
            if (value)
            {
                using var loading = Locator.Current.Resolve<AppLoading>().General.Begin();

                var result = await PmInfoService.LoadAsync();
                if (result.Ok)
                {
                    await AppContextManager.RefreshFromAsync(result.Data!);
                }
            }
        }
        catch (Exception ex)
        {
            LogsWriter.Error(ex, "Processing of online-changing event failed");
        }
    }

    public void OnCompleted()
    {
    }

    public void OnError(Exception error)
    {
    }
}