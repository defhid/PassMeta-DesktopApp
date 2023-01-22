namespace PassMeta.DesktopApp.Common.Abstractions.Services;

using System.Threading.Tasks;
using PassMeta.DesktopApp.Common.Models.Dto.Request;

/// <summary>
/// Service for working with user account data.
/// </summary>
public interface IAccountService
{
    /// <summary>
    /// Refresh information about current authorized user.
    /// </summary>
    public Task<IResult> RefreshUserDataAsync();
        
    /// <summary>
    /// Edit information about current authorized user.
    /// </summary>
    public Task<IResult> UpdateUserDataAsync(UserPatchData data);
}