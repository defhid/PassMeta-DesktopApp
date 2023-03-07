namespace PassMeta.DesktopApp.Common.Abstractions.Services;

using System.Threading.Tasks;
using Models.Dto.Request;
using PassMeta.DesktopApp.Common.Models.Entities;

/// <summary>
/// Service for authorization in PassMeta system.
/// </summary>
public interface IAuthService
{
    /// <summary>
    /// Logging in.
    /// </summary>
    Task<IResult<User>> LogInAsync(SignInPostData data);
        
    /// <summary>
    /// Logging out.
    /// </summary>
    Task LogOutAsync();
                
    /// <summary>
    /// Reset all sessions.
    /// </summary>
    Task ResetAllExceptMeAsync();

    /// <summary>
    /// Registration.
    /// </summary>
    Task<IResult> RegisterAsync(SignUpPostData data);
}