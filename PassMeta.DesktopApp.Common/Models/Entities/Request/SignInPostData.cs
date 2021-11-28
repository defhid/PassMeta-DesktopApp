namespace PassMeta.DesktopApp.Common.Models.Entities.Request
{
    using Newtonsoft.Json;
    
    public class SignInPostData
    {
        [JsonProperty("login")]
        public string Login { get; set; }
        
        [JsonProperty("password")]
        public string Password { get; set; }

        public SignInPostData(string login, string password)
        {
            Login = login;
            Password = password;
        }
    }
}