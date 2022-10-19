namespace PassMeta.DesktopApp.Core.Utils
{
    using System.Threading.Tasks;
    using Common.Abstractions.Services;

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
        public static async Task LoadContextAsync()
        {
            await AppContext.LoadAndSetCurrentAsync();
            await AppContext.RefreshCurrentFromServerAsync();
        }

        /// <summary></summary>
        public static async Task CheckSystemAsync()
        {
            await Task.Run(EnvironmentContainer.Resolve<ILogService>().OptimizeLogs);
        }
    }
}