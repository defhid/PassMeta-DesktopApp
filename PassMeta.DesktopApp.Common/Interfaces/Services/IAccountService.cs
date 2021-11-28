namespace PassMeta.DesktopApp.Common.Interfaces.Services
{
    using DesktopApp.Common.Models;
    using DesktopApp.Common.Models.Entities;
    using DesktopApp.Common.Models.Entities.Request;
    using System.Threading.Tasks;

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