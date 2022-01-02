namespace PassMeta.DesktopApp.Common.Interfaces.Services
{
    using DesktopApp.Common.Models;
    using System.Collections.Generic;
    
    /// <summary>
    /// Service that provides localized messages from <see cref="OkBadResponse"/>.
    /// </summary>
    public interface IOkBadService
    {
        /// <summary>
        /// Get localized message by current culture and loaded translate packages.
        /// </summary>
        string GetLocalizedMessage(string message);

        /// <summary>
        /// Show to user localized failure message by <see cref="OkBadResponse"/>.
        /// </summary>
        /// <param name="response">Response to show.</param>
        /// <param name="whatMapper">Translate package for <see cref="OkBadResponse.What"/> response sections.</param>
        void ShowResponseFailure(OkBadResponse response, IReadOnlyDictionary<string, string>? whatMapper = null);
    }
}