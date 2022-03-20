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
            PassFileManager.Initialize();
            
            await AppContext.LoadAndSetCurrentAsync();
            await AppConfig.LoadAndSetCurrentAsync();

            EnvironmentContainer.Resolve<ILogService>().OptimizeLogs();
        }
    }
}