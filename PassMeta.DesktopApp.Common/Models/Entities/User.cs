namespace PassMeta.DesktopApp.Common.Models.Entities
{
    using Newtonsoft.Json;
    
    /// <summary>
    /// PassMeta user model.
    /// </summary>
    public class User
    {
        private string? _login;
        private string? _fullName;
        
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
        /// User full name.
        /// </summary>
        [JsonProperty("full_name")]
        public string FullName
        {
            get => _fullName ??= string.Empty;
            set => _fullName = value;
        }
    }
}