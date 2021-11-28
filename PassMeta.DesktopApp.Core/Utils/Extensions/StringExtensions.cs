namespace PassMeta.DesktopApp.Core.Utils.Extensions
{
    /// <summary>
    /// Extension methods for <see cref="string"/>.
    /// </summary>
    public static class StringExtensions
    {
        /// <summary>
        /// Make first char uppercase, if string is not null or empty.
        /// </summary>
        public static string Capitalize(this string? str)
        {
            if (string.IsNullOrEmpty(str)) return string.Empty;
            return str[..1].ToUpper() + str[1..];
        }
    }
}