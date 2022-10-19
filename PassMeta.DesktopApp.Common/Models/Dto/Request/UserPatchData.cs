namespace PassMeta.DesktopApp.Common.Models.Dto.Request
{
    using Newtonsoft.Json;

    /// <summary>
    /// Data to send for changing user data.
    /// </summary>
    public class UserPatchData
    {
        ///
        [JsonProperty("login")]
        public string? Login { get; set; }
        
        ///
        [JsonProperty("password")]
        public string? Password { get; set; }
        
        ///
        [JsonProperty("password_confirm")]
        public string? PasswordConfirm { get; set; }
        
        ///
        [JsonProperty("full_name")]
        public string? FullName { get; set; }
    }
}