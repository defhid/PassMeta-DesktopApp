namespace PassMeta.DesktopApp.Common.Interfaces.Services
{
    using DesktopApp.Common.Models;
    using DesktopApp.Common.Models.Entities;
    using System.Threading.Tasks;
    using Models.Dto.Request;

    /// <summary>
    /// Service for working with user account data.
    /// </summary>
    public interface IAccountService
    {
        /// <summary>
        /// Get information about current authorized user.
        /// </summary>
        public Task<Result<User>> GetUserDataAsync();
        
        /// <summary>
        /// Edit information about current authorized user.
        /// </summary>
        public Task<Result> UpdateUserDataAsync(UserPatchData data);
    }
}