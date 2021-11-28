namespace PassMeta.DesktopApp.Common.Models.Entities.Request
{
    using Newtonsoft.Json;
    
    public class SignUpPostData : SignInPostData
    {
        [JsonProperty("first_name")]
        public string FirstName { get; set; }
        
        [JsonProperty("last_name")]
        public string LastName { get; set; }

        public SignUpPostData(string login, string password, string firstName, string lastName) 
            : base(login, password)
        {
            FirstName = firstName;
            LastName = lastName;
        }
    }
}