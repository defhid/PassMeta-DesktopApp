namespace PassMeta.DesktopApp.Core.Utils.Extensions
{
    using System.Net;

    /// <summary>
    /// Extension methods for <see cref="HttpWebRequest"/>.
    /// </summary>
    public static class HttpWebRequestExtensions
    {
        /// <summary>
        /// Get method and url in one line.
        /// </summary>
        public static string GetShortInformation(this HttpWebRequest request)
            => $"{request.Method} \"{request.RequestUri}\"";
    }
}