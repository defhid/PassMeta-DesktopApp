namespace PassMeta.DesktopApp.Core.Services
{
    using DesktopApp.Common;
    using DesktopApp.Common.Models;
    using DesktopApp.Common.Models.Entities;
    using DesktopApp.Common.Models.Dto.Request;
    using DesktopApp.Common.Interfaces.Services;
    using DesktopApp.Common.Utils.Mapping;
    using DesktopApp.Core.Utils;
    using System.Threading.Tasks;
    using Common.Interfaces;

    /// <inheritdoc />
    public class AuthService : IAuthService
    {
        private static readonly SimpleMapper<string, string> WhatToStringMapper = AccountService.WhatToStringMapper + new MapToResource<string>[]
        {
            new("user", () => Resources.DICT_AUTH__USER),
        };
        
        private readonly IDialogService _dialogService = EnvironmentContainer.Resolve<IDialogService>();

        /// <inheritdoc />
        public async Task<IResult<User>> SignInAsync(SignInPostData data)
        {
            if (!_Validate(data).Ok)
            {
                _dialogService.ShowError(Resources.AUTH__DATA_VALIDATION_ERR);
                return Result.Failure<User>();
            }
            
            var response = await PassMetaApi.Post("auth/sign-in", data)
                .WithBadMapping(WhatToStringMapper)
                .WithBadHandling()
                .ExecuteAsync<User>();
            
            if (response?.Success is not true)
                return Result.Failure<User>();
            
            await AppContext.SetUserAsync(response.Data!);
            
            return Result.Success(response.Data!);
        }

        /// <inheritdoc />
        public async Task SignOutAsync()
        {
            var answer = await _dialogService.ConfirmAsync(Resources.ACCOUNT__SIGN_OUT_CONFIRM);
            if (answer.Bad) return;

            await AppContext.SetUserAsync(null);
        }

        /// <inheritdoc />
        public async Task ResetAllExceptMeAsync()
        {
            var answer = await _dialogService.ConfirmAsync(Resources.ACCOUNT__RESET_SESSIONS_CONFIRM);
            if (answer.Bad) return;

            var response = await PassMetaApi.Post("auth/reset/all-except-me")
                .WithBadHandling()
                .ExecuteAsync();

            if (response?.Success is true)
            {
                _dialogService.ShowInfo(Resources.ACCOUNT__RESET_SESSIONS_SUCCESS);
            }
        }

        /// <inheritdoc />
        public async Task<IResult<User>> SignUpAsync(SignUpPostData data)
        {
            if (!_Validate(data).Ok)
            {
                _dialogService.ShowError(Resources.AUTH__DATA_VALIDATION_ERR);
                return Result.Failure<User>();
            }
            
            var response = await PassMetaApi.Post("users/new", data)
                .WithBadMapping(WhatToStringMapper)
                .WithBadHandling()
                .ExecuteAsync<User>();
            
            if (response?.Success is not true) 
                return Result.Failure<User>();
                
            await AppContext.SetUserAsync(response.Data);
            return Result.Success(response.Data!);
        }
        
        private static IResult<TData> _Validate<TData>(TData data)
            where TData : SignInPostData
        {
            if (data.Login.Length < 1 || data.Password.Length < 1)
            {
                return Result.Failure<TData>();
            }

            if (data is SignUpPostData signUpData)
            {
                if (signUpData.FullName.Length < 1)
                {
                    return Result.Failure<TData>();
                }
            }

            return Result.Success(data);
        }
    }
}