namespace PassMeta.DesktopApp.Common.Utils.Extensions
{
    using System;

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

        /// <summary>
        /// Add text after <see cref="Environment.NewLine"/>.
        /// </summary>
        public static string NewLine(this string str, string? addNewLine)
        {
            return addNewLine is null 
                ? str 
                : str + Environment.NewLine + addNewLine;
        }
    }
}