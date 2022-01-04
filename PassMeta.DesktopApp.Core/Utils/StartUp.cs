namespace PassMeta.DesktopApp.Core.Utils
{
    using System.Threading.Tasks;

    /// <summary>
    /// Checking and loading at startup.
    /// </summary>
    public static class StartUp
    {
        /// <summary>
        /// Method to call at startup.
        /// </summary>
        public static async Task CheckSystemAndLoadApplicationConfigAsync()
        {
            PassFileLocalManager.Initialize();
            
            await AppContext.LoadAndSetCurrentAsync();
            await AppConfig.LoadAndSetCurrentAsync();
        }
    }
}