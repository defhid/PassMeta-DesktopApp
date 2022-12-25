using System.Net.Http;

namespace PassMeta.DesktopApp.Core.Utils.Extensions;

/// <summary>
/// Extension methods for <see cref="HttpRequestMessage"/>.
/// </summary>
internal static class HttpRequestMessageExtensions
{
    /// <summary>
    /// Get method and url in one line.
    /// </summary>
    public static string GetShortInformation(this HttpRequestMessage request)
        => $"{request.Method} \"{request.RequestUri}\"";
}