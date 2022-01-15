namespace PassMeta.DesktopApp.Common.Utils.Extensions
{
    using System;

    /// <summary>
    /// Extension methdos for <see cref="DateTime"/>.
    /// </summary>
    public static class DateTimeExtensions
    {
        /// <summary>
        /// Concat short date and short time through a <paramref name="separator"/>.
        /// </summary>
        public static string ToShortDateTimeString(this DateTime dt, string separator = " ")
            => dt.ToString("d", Resources.Culture) + separator + dt.ToString("t", Resources.Culture);
    }
}