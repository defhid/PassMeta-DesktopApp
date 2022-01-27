namespace PassMeta.DesktopApp.Common.Utils.Extensions
{
    using Models;

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
                : response.What is null 
                    ? response.Message 
                    : $"{response.Message}: {response.What}";
        }
    }
}