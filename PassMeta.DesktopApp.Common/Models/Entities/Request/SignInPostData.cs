using System.Diagnostics.CodeAnalysis;
using Newtonsoft.Json;

namespace PassMeta.DesktopApp.Common.Models.Entities.Request
{
    public class SignInPostData
    {
        [JsonProperty("login")]
        [NotNull]
        public string Login { get; set; }
        
        [JsonProperty("password")]
        [NotNull]
        public string Password { get; set; }
    }
}