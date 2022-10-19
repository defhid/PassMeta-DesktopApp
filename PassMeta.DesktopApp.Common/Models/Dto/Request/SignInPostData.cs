namespace PassMeta.DesktopApp.Common.Models.Dto.Request
{
    using Newtonsoft.Json;

    /// <summary>
    /// Data to send for login.
    /// </summary>
    public class SignInPostData
    {
        ///
        [JsonProperty("login")]
        public string Login { get; set; }
        
        ///
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