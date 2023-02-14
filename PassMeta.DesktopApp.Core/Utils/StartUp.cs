using PassMeta.DesktopApp.Common.Abstractions.Utils.PassMetaClient;
using System.Threading.Tasks;
using PassMeta.DesktopApp.Common;
using PassMeta.DesktopApp.Common.Abstractions.AppConfig;
using PassMeta.DesktopApp.Common.Abstractions.AppContext;
using PassMeta.DesktopApp.Common.Abstractions.Services;
using PassMeta.DesktopApp.Common.Abstractions.Utils.Logging;
using PassMeta.DesktopApp.Core.Extensions;
using Splat;

namespace PassMeta.DesktopApp.Core.Utils;

/// <summary>
/// Loading and optimizing system at startup.
/// </summary>
public static class StartUp
{
    /// <summary></summary>
    public static async Task LoadAsync()
    {
        await Locator.Current.Resolve<IAppConfigManager>().LoadAsync();
        await Locator.Current.Resolve<IAppContextManager>().LoadAsync();

        var passMetaClient = Locator.Current.Resolve<IPassMetaClient>();

        if (!await passMetaClient.CheckConnectionAsync())
        {
            Locator.Current.Resolve<IDialogService>().ShowInfo(Resources.API__CONNECTION_ERR);
        }
    }

    /// <summary></summary>
    public static void CleanUp()
    {
        Locator.Current.Resolve<ILogsManager>().CleanUp();
    }
}