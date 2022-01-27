namespace PassMeta.DesktopApp.Common.Interfaces.Services
{
    using DesktopApp.Common.Models;

    /// <summary>
    /// Service that provides human-readable content from <see cref="OkBadResponse"/>.
    /// </summary>
    public interface IOkBadService
    {
        /// <summary>
        /// Show to user failure message by <see cref="OkBadResponse"/>.
        /// </summary>
        /// <param name="response">Response to show.</param>
        void ShowResponseFailure(OkBadResponse response);
    }
}