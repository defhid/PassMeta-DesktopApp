namespace PassMeta.DesktopApp.Core.Utils
{
    using System.Threading.Tasks;
    using Common.Interfaces.Services;

    /// <summary>
    /// Checking, loading and optimizing at startup.
    /// </summary>
    public static class StartUp
    {
        /// <summary></summary>
        public static async Task LoadConfigurationAsync()
        {
            await AppConfig.LoadAndSetCurrentAsync();
        }

        /// <summary></summary>
        public static async Task LoadContextAndCheckSystemAsync()
        {
            var backgroundTask = Task.Run(async () =>
            {
                await PassFileManager.ReloadAsync(true);
                EnvironmentContainer.Resolve<ILogService>().OptimizeLogs();
            });

            await AppContext.LoadAndSetCurrentAsync();
            await AppContext.RefreshFromServerAsync();

            await backgroundTask;
        }
    }
}