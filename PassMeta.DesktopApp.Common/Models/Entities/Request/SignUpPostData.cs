using System.Diagnostics.CodeAnalysis;
using Newtonsoft.Json;

namespace PassMeta.DesktopApp.Common.Models.Entities.Request
{
    public class SignUpPostData
    {
        [JsonProperty("login")]
        [NotNull]
        public string Login { get; set; }
        
        [JsonProperty("password")]
        [NotNull]
        public string Password { get; set; }
        
        [JsonProperty("first_name")]
        [NotNull]
        public string FirstName { get; set; }
        
        [JsonProperty("last_name")]
        [NotNull]
        public string LastName { get; set; }
    }
}