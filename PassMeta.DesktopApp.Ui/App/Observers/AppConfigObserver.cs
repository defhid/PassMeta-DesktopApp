using System;
using System.Globalization;
using System.Threading;
using PassMeta.DesktopApp.Common;
using PassMeta.DesktopApp.Common.Abstractions.AppConfig;
using PassMeta.DesktopApp.Common.Abstractions.Utils.Logging;
using PassMeta.DesktopApp.Common.Abstractions.Utils.PassMetaClient;
using PassMeta.DesktopApp.Common.Constants;
using PassMeta.DesktopApp.Core.Extensions;
using PassMeta.DesktopApp.Core.Services.Extensions;
using Splat;

namespace PassMeta.DesktopApp.Ui.App.Observers;

public class AppConfigObserver : IObserver<IAppConfig>
{
    private static IPassMetaClient PassMetaClient => Locator.Current.Resolve<IPassMetaClient>();
    private static ILogsWriter LogsWriter => Locator.Current.Resolve<ILogsWriter>();

    private IAppConfig? _prev;

    public async void OnNext(IAppConfig value)
    {
        try
        {
            if (_prev is null)
            {
                SetCulture(value.Culture);
                _prev = value;
                return;
            }

            if (value.Culture != _prev.Culture)
            {
                SetCulture(value.Culture);
            }

            if (value.ServerUrl != _prev.ServerUrl)
            {
                using var loading = AppLoading.General.Begin();

                await PassMetaClient.CheckConnectionAsync(reset: true);
            }
        }
        catch (Exception ex)
        {
            LogsWriter.Error(ex, "Processing of app-config-changing event failed");
        }

        _prev = value;
    }

    public void OnCompleted()
    {
    }

    public void OnError(Exception error)
    {
    }

    private static void SetCulture(AppCulture culture)
    {
        Resources.Culture = culture;

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
}