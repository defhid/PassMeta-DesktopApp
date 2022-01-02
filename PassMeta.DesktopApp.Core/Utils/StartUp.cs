namespace PassMeta.DesktopApp.Core.Utils
{
    /// <summary>
    /// Checking and loading at startup.
    /// </summary>
    public static class StartUp
    {
        /// <summary>
        /// Method to call at startup.
        /// </summary>
        public static void CheckSystemAndLoadApplicationConfig()
        {
            PassFileLocalManager.Initialize();
            
            AppConfig.LoadAndSetCurrentAsync().GetAwaiter().GetResult();
        }
    }
}