namespace PassMeta.DesktopApp.Core.Utils.Extensions
{
    using Common.Interfaces.Services;
    using Common.Models;
    using Splat;

    /// <summary>
    /// Extension methods for <see cref="OkBadResponse"/>.
    /// </summary>
    public static class OkBadResponseExtensions
    {
        /// <summary>
        /// Get localized message from optional response using <see cref="IOkBadService"/>.
        /// </summary>
        public static string GetLocalizedMessage(this OkBadResponse? response)
        {
            return response is null 
                ? string.Empty 
                : Locator.Current.GetService<IOkBadService>()!.GetLocalizedMessage(response.Message);
        }
    }
}