using System.Diagnostics.CodeAnalysis;
using Newtonsoft.Json;

namespace PassMeta.DesktopApp.Common.Models.Entities.Request
{
    public class SignUpPostData : SignInPostData
    {
        [JsonProperty("first_name")]
        [NotNull]
        public string FirstName { get; set; }
        
        [JsonProperty("last_name")]
        [NotNull]
        public string LastName { get; set; }
    }
}