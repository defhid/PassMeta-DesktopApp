namespace PassMeta.DesktopApp.Core.Utils
{
    using System.Threading.Tasks;
    using Common.Interfaces.Services;

    /// <summary>
    /// Checking, loading and optimizing at startup.
    /// </summary>
    public static class StartUp
    {
        /// <summary>
        /// Method to call at startup.
        /// </summary>
        public static async Task CheckSystemAndLoadApplicationConfigAsync()
        {
            await AppContext.LoadAndSetCurrentAsync();
            await AppConfig.LoadAndSetCurrentAsync();

            await PassFileManager.ReloadAsync(true);

            EnvironmentContainer.Resolve<ILogService>().OptimizeLogs();
        }
    }
}