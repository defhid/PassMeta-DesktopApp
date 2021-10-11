using System.Threading.Tasks;
using PassMeta.DesktopApp.Common.Models;
using PassMeta.DesktopApp.Common.Models.Entities;
using PassMeta.DesktopApp.Common.Models.Entities.Request;

namespace PassMeta.DesktopApp.Common.Interfaces.Services
{
    public interface IAuthService
    {
        /// <summary>
        /// Logging in.
        /// </summary>
        Task<Result<User>> SignInAsync(SignInPostData data);
        
        /// <summary>
        /// Logging out.
        /// </summary>
        Task SignOutAsync();

        /// <summary>
        /// Registration.
        /// </summary>
        Task<Result<User>> SignUpAsync(SignUpPostData data);
    }
}