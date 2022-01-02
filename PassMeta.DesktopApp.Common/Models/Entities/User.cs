namespace PassMeta.DesktopApp.Common.Models.Entities
{
    using Newtonsoft.Json;
    
    /// <summary>
    /// PassMeta user model.
    /// </summary>
    public class User
    {
        private string? _login;
        private string? _firstName;
        private string? _lastName;
        
        /// <summary>
        /// User identifier.
        /// </summary>
        [JsonProperty("id")]
        public int Id { get; set; }
        
        /// <summary>
        /// User login.
        /// </summary>
        [JsonProperty("login")]
        public string Login { 
            get => _login ??= string.Empty;
            set => _login = value;
        }

        /// <summary>
        /// User first name.
        /// </summary>
        [JsonProperty("first_name")]
        public string FirstName
        {
            get => _firstName ??= string.Empty;
            set => _firstName = value;
        }

        /// <summary>
        /// User last name.
        /// </summary>
        [JsonProperty("last_name")]
        public string LastName
        {
            get => _lastName ??= string.Empty;
            set => _lastName = value;
        }
    }
}