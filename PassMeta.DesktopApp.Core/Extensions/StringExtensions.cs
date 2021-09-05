using System.Diagnostics.CodeAnalysis;

namespace PassMeta.DesktopApp.Core.Extensions
{
    public static class StringExtensions
    {
        public static string Capitalize([AllowNull] this string str)
        {
            if (string.IsNullOrEmpty(str)) return string.Empty;
            return str[..1].ToUpper() + str[1..];
        }
    }
}