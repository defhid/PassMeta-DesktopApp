namespace PassMeta.DesktopApp.Common.Models.Dto.Request;

/// <summary>
/// Data to send for a new user registration.
/// </summary>
public class SignUpPostData : SignInPostData
{
    ///
    public string FullName { get; set; }

    /// <summary></summary>
    public SignUpPostData(string login, string password, string fullName) 
        : base(login, password)
    {
        FullName = fullName;
    }
}