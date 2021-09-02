using System.Threading.Tasks;
using PassMeta.DesktopApp.Common.Models;
using PassMeta.DesktopApp.Common.Models.Entities.Request;

namespace PassMeta.DesktopApp.Common.Interfaces.Services
{
    public interface IAccountService
    {
        /// <summary>
        /// Edit information about current authorized user.
        /// </summary>
        public Task<Result> UpdateUserData(UserPatchData data);
    }
}