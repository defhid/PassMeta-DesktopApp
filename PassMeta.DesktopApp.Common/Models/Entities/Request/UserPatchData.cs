namespace PassMeta.DesktopApp.Common.Models.Entities.Request
{
    using Newtonsoft.Json;
    
    public class UserPatchData
    {
        [JsonProperty("login")]
        public string? Login { get; set; }
        
        [JsonProperty("password")]
        public string? Password { get; set; }
        
        [JsonProperty("password_confirm")]
        public string? PasswordConfirm { get; set; }
        
        [JsonProperty("first_name")]
        public string? FirstName { get; set; }
        
        [JsonProperty("last_name")]
        public string? LastName { get; set; }
    }
}