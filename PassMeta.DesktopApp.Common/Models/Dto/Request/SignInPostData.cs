namespace PassMeta.DesktopApp.Common.Models.Dto.Request
{
    using Newtonsoft.Json;

    /// <summary>
    /// Data to send for login.
    /// </summary>
    public class SignInPostData
    {
        /// <summary>
        /// User login.
        /// </summary>
        [JsonProperty("login")]
        public string Login { get; set; }
        
        /// <summary>
        /// User password.
        /// </summary>
        [JsonProperty("password")]
        public string Password { get; set; }

        /// <summary></summary>
        public SignInPostData(string login, string password)
        {
            Login = login;
            Password = password;
        }
    }
}