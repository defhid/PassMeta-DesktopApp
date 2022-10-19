namespace PassMeta.DesktopApp.Ui.Utils
{
    using Core;
    using Core.Utils;

    public static class PresetsCache
    {
        public static class Generator
        {
            public static int Length = AppConfig.Current.DefaultPasswordLength;
            
            public static bool IncludeDigits = true;

            public static bool IncludeLowercase = true;

            public static bool IncludeUppercase = true;

            public static bool IncludeSpecial = true;
        }
    }
}