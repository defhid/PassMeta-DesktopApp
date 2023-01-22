using PassMeta.DesktopApp.Common.Models.Dto.Response.OkBad;

namespace PassMeta.DesktopApp.Common.Extensions;

/// <summary>
/// Extension methods for <see cref="OkBadResponse"/>.
/// </summary>
public static class OkBadResponseExtensions
{
    /// <summary>
    /// Get message with short additional information from optional response.
    /// </summary>
    public static string GetFullMessage(this OkBadResponse? response)
    {
        return response is null 
            ? string.Empty 
            : response.More?.What is null 
                ? response.Message 
                : $"{response.More.What}: {response.Message}";
    }
}