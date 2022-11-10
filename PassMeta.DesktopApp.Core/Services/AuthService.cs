namespace PassMeta.DesktopApp.Core.Services
{
    using DesktopApp.Common;
    using DesktopApp.Common.Models;
    using DesktopApp.Common.Models.Entities;
    using DesktopApp.Common.Models.Dto.Request;
    using DesktopApp.Common.Utils.Mapping;
    using System.Threading.Tasks;
    using Common.Abstractions;
    using Common.Abstractions.Services;
    using Common.Abstractions.Utils;

    /// <inheritdoc />
    public class AuthService : IAuthService
    {
        private static readonly SimpleMapper<string, string> WhatToStringMapper = AccountService.WhatToStringMapper + new MapToResource<string>[]
        {
            new("user", () => Resources.DICT_AUTH__USER),
        };
        
        private readonly IPassMetaClient _passMetaClient;
        private readonly IDialogService _dialogService;

        /// <summary></summary>
        public AuthService(IDialogService dialogService, IPassMetaClient passMetaClient)
        {
            _dialogService = dialogService;
            _passMetaClient = passMetaClient;
        }

        /// <inheritdoc />
        public async Task<IResult<User>> SignInAsync(SignInPostData data)
        {
            if (!_Validate(data).Ok)
            {
                _dialogService.ShowError(Resources.AUTH__DATA_VALIDATION_ERR);
                return Result.Failure<User>();
            }
            
            var response = await _passMetaClient.Post("auth/sign-in")
                .WithJsonBody(data)
                .WithBadMapping(WhatToStringMapper)
                .WithBadHandling()
                .ExecuteAsync<User>();
            
            if (response?.Success is not true)
                return Result.Failure<User>();

            AppContext.Current.User = response.Data!;
            await AppContext.FlushCurrentAsync();
            
            return Result.Success(response.Data!);
        }

        /// <inheritdoc />
        public async Task SignOutAsync()
        {
            var answer = await _dialogService.ConfirmAsync(Resources.ACCOUNT__SIGN_OUT_CONFIRM);
            if (answer.Bad) return;

            AppContext.Current.User = null;
            await AppContext.FlushCurrentAsync();
        }

        /// <inheritdoc />
        public async Task ResetAllExceptMeAsync()
        {
            var answer = await _dialogService.ConfirmAsync(Resources.ACCOUNT__RESET_SESSIONS_CONFIRM);
            if (answer.Bad) return;

            var response = await _passMetaClient.Post("auth/reset/all-except-me")
                .WithBadHandling()
                .ExecuteAsync();

            if (response?.Success is true)
            {
                _dialogService.ShowInfo(Resources.ACCOUNT__RESET_SESSIONS_SUCCESS);
            }
        }

        /// <inheritdoc />
        public async Task<IResult> SignUpAsync(SignUpPostData data)
        {
            if (!_Validate(data).Ok)
            {
                _dialogService.ShowError(Resources.AUTH__DATA_VALIDATION_ERR);
                return Result.Failure<User>();
            }
            
            var response = await _passMetaClient.Post("users/new")
                .WithJsonBody(data)
                .WithBadMapping(WhatToStringMapper)
                .WithBadHandling()
                .ExecuteAsync<User>();
            
            if (response?.Success is not true) 
                return Result.Failure<User>();

            AppContext.Current.User = response.Data!;
            await AppContext.FlushCurrentAsync();

            return Result.Success();
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