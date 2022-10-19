namespace PassMeta.DesktopApp.Common.Abstractions.Services
{
    using PassMeta.DesktopApp.Common.Models;

    /// <summary>
    /// Service that provides human-readable content from <see cref="OkBadResponse"/>.
    /// </summary>
    public interface IOkBadService
    {
        /// <summary>
        /// Show to user failure message by <see cref="OkBadResponse"/>.
        /// </summary>
        /// <param name="response">Response to show.</param>
        /// <param name="context">Request context.</param>
        void ShowResponseFailure(OkBadResponse response, string? context = null);
    }
}