namespace PassMeta.DesktopApp.Common.Interfaces.Services
{
    using DesktopApp.Common.Models.Entities;
    using System.Threading.Tasks;
    using Models.Dto.Request;

    /// <summary>
    /// Service for authorization in PassMeta system.
    /// </summary>
    public interface IAuthService
    {
        /// <summary>
        /// Logging in.
        /// </summary>
        Task<IResult<User>> SignInAsync(SignInPostData data);
        
        /// <summary>
        /// Logging out.
        /// </summary>
        Task SignOutAsync();

        /// <summary>
        /// Registration.
        /// </summary>
        Task<IResult<User>> SignUpAsync(SignUpPostData data);
    }
}