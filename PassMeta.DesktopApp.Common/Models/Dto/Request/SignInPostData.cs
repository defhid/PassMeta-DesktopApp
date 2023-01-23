namespace PassMeta.DesktopApp.Common.Models.Dto.Request;

/// <summary>
/// Data to send for login.
/// </summary>
public class SignInPostData
{
    ///
    public string Login { get; set; }
        
    ///
    public string Password { get; set; }

    /// <summary></summary>
    public SignInPostData(string login, string password)
    {
        Login = login;
        Password = password;
    }
}