namespace PassMeta.DesktopApp.Common.Models.Dto.Request
{
    using Newtonsoft.Json;

    /// <summary>
    /// Data to send for a new user registration.
    /// </summary>
    public class SignUpPostData : SignInPostData
    {
        /// <summary>
        /// User first name.
        /// </summary>
        [JsonProperty("first_name")]
        public string FirstName { get; set; }
        
        /// <summary>
        /// User last name.
        /// </summary>
        [JsonProperty("last_name")]
        public string LastName { get; set; }
        
        /// <summary></summary>
        public SignUpPostData(string login, string password, string firstName, string lastName) 
            : base(login, password)
        {
            FirstName = firstName;
            LastName = lastName;
        }
    }
}