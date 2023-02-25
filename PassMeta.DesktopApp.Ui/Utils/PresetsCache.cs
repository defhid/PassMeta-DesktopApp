namespace PassMeta.DesktopApp.Ui.Utils;

using Core;

public static class PresetsCache
{
    public static class Generator
    {
        public static int Length = AppPaths.Current.DefaultPasswordLength;
            
        public static bool IncludeDigits = true;

        public static bool IncludeLowercase = true;

        public static bool IncludeUppercase = true;

        public static bool IncludeSpecial = true;
    }
}