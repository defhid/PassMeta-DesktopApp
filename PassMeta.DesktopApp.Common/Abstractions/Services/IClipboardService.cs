using System.Threading.Tasks;

namespace PassMeta.DesktopApp.Common.Abstractions.Services;

/// <summary>
/// Service for working with user's clipboard.
/// </summary>
public interface IClipboardService
{
    /// <summary>
    /// Try to set a text to clipboard.
    /// </summary>
    Task<bool> TrySetTextAsync(string? text);
}