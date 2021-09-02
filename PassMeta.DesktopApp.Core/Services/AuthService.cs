using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using PassMeta.DesktopApp.Common.Interfaces.Services;
using PassMeta.DesktopApp.Common.Models;
using PassMeta.DesktopApp.Common.Models.Entities;
using PassMeta.DesktopApp.Common.Models.Entities.Request;

namespace PassMeta.DesktopApp.Core.Services
{
    public class AuthService : IAuthService
    {
        public async Task<Result<User>> SignIn(string login, string password)
        {
            throw new System.NotImplementedException();
        }

        public async Task<Result> SignOut()
        {
            throw new System.NotImplementedException();
        }

        public async Task<Result<User>> SignUp(SignUpPostData postData)
        {
            throw new System.NotImplementedException();
        }
    }
}