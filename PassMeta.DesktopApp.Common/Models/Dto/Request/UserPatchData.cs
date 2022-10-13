namespace PassMeta.DesktopApp.Common.Models.Dto.Request
{
    using Newtonsoft.Json;

    /// <summary>
    /// Data to send for changing user data.
    /// </summary>
    public class UserPatchData
    {
        /// <summary>
        /// User new login.
        /// </summary>
        [JsonProperty("login")]
        public string? Login { get; set; }
        
        /// <summary>
        /// User new password.
        /// </summary>
        [JsonProperty("password")]
        public string? Password { get; set; }
        
        /// <summary>
        /// User current password confirmation.
        /// </summary>
        [JsonProperty("password_confirm")]
        public string? PasswordConfirm { get; set; }
        
        /// <summary>
        /// User new full name.
        /// </summary>
        [JsonProperty("full_name")]
        public string? FullName { get; set; }
    }
}