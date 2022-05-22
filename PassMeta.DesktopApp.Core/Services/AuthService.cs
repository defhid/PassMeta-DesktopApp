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