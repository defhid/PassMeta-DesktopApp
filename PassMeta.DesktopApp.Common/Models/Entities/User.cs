using Newtonsoft.Json;

namespace PassMeta.DesktopApp.Common.Models.Entities
{
    public class User
    {
        private string? _login;
        private string? _firstName;
        private string? _lastName;
        
        [JsonProperty("id")]
        public int Id { get; set; }
        
        [JsonProperty("login")]
        public string Login { 
            get => _login ?? "";
            set => _login = value;
        }

        [JsonProperty("first_name")]
        public string FirstName
        {
            get => _firstName ?? "";
            set => _firstName = value;
        }

        [JsonProperty("last_name")]
        public string LastName
        {
            get => _lastName ?? "";
            set => _lastName = value;
        }
    }
}