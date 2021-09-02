using System.Diagnostics.CodeAnalysis;
using Newtonsoft.Json;

namespace PassMeta.DesktopApp.Common.Models.Entities.Request
{
    public class UserPatchData
    {
        [JsonProperty("login")]
        [AllowNull]
        public string Login { get; set; }
        
        [JsonProperty("password")]
        [AllowNull]
        public string Password { get; set; }
        
        [JsonProperty("password_confirm")]
        [AllowNull]
        public string PasswordConfirm { get; set; }
        
        [JsonProperty("first_name")]
        [AllowNull]
        public string FirstName { get; set; }
        
        [JsonProperty("last_name")]
        [AllowNull]
        public string LastName { get; set; }
    }
}