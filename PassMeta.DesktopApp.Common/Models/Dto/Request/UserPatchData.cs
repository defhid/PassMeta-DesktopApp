namespace PassMeta.DesktopApp.Common.Models.Dto.Request;

/// <summary>
/// Data to send for changing user data.
/// </summary>
public class UserPatchData
{
    ///
    public string? Login { get; set; }
        
    ///
    public string? Password { get; set; }
        
    ///
    public string? PasswordConfirm { get; set; }
        
    ///
    public string? FullName { get; set; }
}