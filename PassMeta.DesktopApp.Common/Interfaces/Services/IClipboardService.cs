namespace PassMeta.DesktopApp.Common.Interfaces.Services
{
    using System.Threading.Tasks;

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
}