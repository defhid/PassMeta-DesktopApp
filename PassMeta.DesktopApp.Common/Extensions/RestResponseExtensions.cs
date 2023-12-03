using PassMeta.DesktopApp.Common.Models.Dto.Response;

namespace PassMeta.DesktopApp.Common.Extensions;

/// <summary>
/// Extension methods for <see cref="RestResponse"/>.
/// </summary>
public static class RestResponseExtensions
{
    /// <summary>
    /// Get message with short additional information from optional response.
    /// </summary>
    public static string GetFullMessage(this RestResponse? response)
    {
        return response is null 
            ? string.Empty 
            : response.More?.Count > 0
                ? response.Message + $" ({string.Join("; ", response.More)})" 
                : response.Message;
    }
}