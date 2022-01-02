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
        /// User new first name.
        /// </summary>
        [JsonProperty("first_name")]
        public string? FirstName { get; set; }
        
        /// <summary>
        /// User new last name.
        /// </summary>
        [JsonProperty("last_name")]
        public string? LastName { get; set; }
    }
}