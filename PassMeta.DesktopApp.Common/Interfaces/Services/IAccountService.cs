using System.Threading.Tasks;
using PassMeta.DesktopApp.Common.Models;
using PassMeta.DesktopApp.Common.Models.Entities;
using PassMeta.DesktopApp.Common.Models.Entities.Request;

namespace PassMeta.DesktopApp.Common.Interfaces.Services
{
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