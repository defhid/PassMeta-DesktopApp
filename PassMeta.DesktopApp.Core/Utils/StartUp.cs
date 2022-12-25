using PassMeta.DesktopApp.Common.Abstractions.Utils.PassMetaClient;
using PassMeta.DesktopApp.Common.Abstractions.Services.Logging;

using System.Threading.Tasks;
using PassMeta.DesktopApp.Common;
using PassMeta.DesktopApp.Common.Abstractions.Services;

namespace PassMeta.DesktopApp.Core.Utils;

/// <summary>
/// Checking, loading and optimizing at startup.
/// </summary>
public static class StartUp
{
    /// <summary></summary>
    public static async Task LoadConfigurationAsync()
    {
        using var loading = AppLoading.General.Begin();
        await AppConfig.LoadAndSetCurrentAsync();
    }

    /// <summary></summary>
    public static async Task LoadContextAsync()
    {
        using var loading = AppLoading.General.Begin();

        await AppContext.LoadAndSetCurrentAsync();

        var passMetaClient = EnvironmentContainer.Resolve<IPassMetaClient>();

        if (!await passMetaClient.CheckConnectionAsync())
        {
            EnvironmentContainer.Resolve<IDialogService>().ShowInfo(Resources.API__CONNECTION_ERR);
        }
    }

    /// <summary></summary>
    public static void CheckSystem()
    {
        using var loading = AppLoading.GeneralBackground.Begin();
        EnvironmentContainer.Resolve<ILogService>().CleanUp();
    }
}