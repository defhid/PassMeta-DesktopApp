using System.Diagnostics.CodeAnalysis;
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
        Task<Result<User>> SignIn([NotNull] string login, [NotNull] string password);
        
        /// <summary>
        /// Logging out.
        /// </summary>
        Task<Result> SignOut();

        /// <summary>
        /// Registration.
        /// </summary>
        Task<Result<User>> SignUp([NotNull] SignUpPostData postData);
    }
}