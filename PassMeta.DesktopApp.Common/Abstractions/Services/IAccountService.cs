namespace PassMeta.DesktopApp.Common.Abstractions.Services
{
    using System.Threading.Tasks;
    using PassMeta.DesktopApp.Common.Models.Dto.Request;
    using PassMeta.DesktopApp.Common.Models.Entities;

    /// <summary>
    /// Service for working with user account data.
    /// </summary>
    public interface IAccountService
    {
        /// <summary>
        /// Get information about current authorized user.
        /// </summary>
        public Task<IResult<User>> GetUserDataAsync();
        
        /// <summary>
        /// Edit information about current authorized user.
        /// </summary>
        public Task<IResult> UpdateUserDataAsync(UserPatchData data);
    }
}