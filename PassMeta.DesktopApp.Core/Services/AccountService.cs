using System.Threading.Tasks;
using PassMeta.DesktopApp.Common.Interfaces;
using PassMeta.DesktopApp.Common.Interfaces.Services;
using PassMeta.DesktopApp.Common.Models;
using PassMeta.DesktopApp.Common.Models.Entities.Request;

namespace PassMeta.DesktopApp.Core.Services
{
    public class AccountService : IAccountService
    {
        public async Task<Result> UpdateUserData(UserPatchData data)
        {
            throw new System.NotImplementedException();
        }
    }
}