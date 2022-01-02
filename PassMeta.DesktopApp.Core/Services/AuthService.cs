namespace PassMeta.DesktopApp.Core.Services
{
    using DesktopApp.Common.Interfaces.Services;
    using DesktopApp.Common.Models;
    using DesktopApp.Common.Models.Entities;
    using DesktopApp.Core.Utils;
    using System.Threading.Tasks;
    using Common.Models.Dto.Request;
    using Splat;
    
    /// <inheritdoc />
    public class AuthService : IAuthService
    {
        private readonly IDialogService _dialogService = Locator.Current.GetService<IDialogService>()!;

        /// <inheritdoc />
        public async Task<Result<User>> SignInAsync(SignInPostData data)
        {
            if (!_Validate(data).Ok)
            {
                _dialogService.ShowError(Common.Resources.AUTHERR__DATA_VALIDATION_ERR);
                return Result.Failure<User>();
            }
            
            var response = await PassMetaApi.Post("/auth/sign-in", data)
                .WithBadHandling()
                .ExecuteAsync<User>();
            
            if (response?.Success is not true)
                return Result.Failure<User>();
            
            await AppConfig.Current.SetUserAsync(response.Data);
            return Result.Success(response.Data!);
        }

        /// <inheritdoc />
        public async Task SignOutAsync()
        {
            var answer = await _dialogService.ConfirmAsync(Common.Resources.ACCOUNT__SIGN_OUT_CONFIRM);
            if (answer.Bad) return;
            
            var response = await PassMetaApi.Post("/auth/sign-out").WithBadHandling().ExecuteAsync();
            if (response?.Success is not true) return;
            
            await AppConfig.Current.SetUserAsync(null);
        }

        /// <inheritdoc />
        public async Task<Result<User>> SignUpAsync(SignUpPostData data)
        {
            if (!_Validate(data).Ok)
            {
                _dialogService.ShowError(Common.Resources.AUTHERR__DATA_VALIDATION_ERR);
                return Result.Failure<User>();
            }
            
            var response = await PassMetaApi.Post("/users/new", data).WithBadHandling().ExecuteAsync<User>();
            if (response?.Success is not true) 
                return Result.Failure<User>();
                
            await AppConfig.Current.SetUserAsync(response.Data);
            return Result.Success(response.Data!);
        }
        
        private static Result<TData> _Validate<TData>(TData data)
            where TData : SignInPostData
        {
            data.Login = data.Login.Trim();
            
            if (data.Login.Length < 1 || data.Password.Length < 1)
            {
                return Result.Failure<TData>();
            }

            if (data is SignUpPostData signUpData)
            {
                signUpData.FirstName = signUpData.FirstName.Trim();
                signUpData.LastName = signUpData.LastName.Trim();
                
                if (signUpData.FirstName.Length < 1 || signUpData.LastName.Length < 1)
                {
                    return Result.Failure<TData>();
                }
            }

            return Result.Success(data);
        }
    }
}