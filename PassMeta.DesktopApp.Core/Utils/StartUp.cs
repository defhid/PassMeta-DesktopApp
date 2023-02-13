using PassMeta.DesktopApp.Common.Abstractions.Utils.PassMetaClient;
using System.Threading.Tasks;
using PassMeta.DesktopApp.Common;
using PassMeta.DesktopApp.Common.Abstractions.AppConfig;
using PassMeta.DesktopApp.Common.Abstractions.AppContext;
using PassMeta.DesktopApp.Common.Abstractions.Services;
using PassMeta.DesktopApp.Common.Abstractions.Utils.Logging;

namespace PassMeta.DesktopApp.Core.Utils;

/// <summary>
/// Loading and optimizing system at startup.
/// </summary>
public static class StartUp
{
    /// <summary></summary>
    public static async Task LoadAsync()
    {
        using var loading = AppLoading.General.Begin();

        await EnvironmentContainer.Resolve<IAppConfigManager>().LoadAsync();
        await EnvironmentContainer.Resolve<IAppContextManager>().LoadAsync();

        var passMetaClient = EnvironmentContainer.Resolve<IPassMetaClient>();

        if (!await passMetaClient.CheckConnectionAsync())
        {
            EnvironmentContainer.Resolve<IDialogService>().ShowInfo(Resources.API__CONNECTION_ERR);
        }
    }

    /// <summary></summary>
    public static void CleanUp()
    {
        using var loading = AppLoading.GeneralBackground.Begin();
        EnvironmentContainer.Resolve<ILogsManager>().CleanUp();
    }
}