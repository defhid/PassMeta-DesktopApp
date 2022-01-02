namespace PassMeta.DesktopApp.Core.Services
{
    using DesktopApp.Common.Interfaces.Services;
    using DesktopApp.Common.Models;
    using DesktopApp.Common.Models.Entities;
    using DesktopApp.Core.Utils;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Common.Models.Dto.Request;
    using Splat;
    
    /// <inheritdoc />
    public class AccountService : IAccountService
    {
        private static Dictionary<string, string> WhatMapper => new()
        {
            ["first_name"] = Common.Resources.ACCOUNT__FIRST_NAME_LABEL.ToLower(),
            ["last_name"] = Common.Resources.ACCOUNT__LAST_NAME_LABEL.ToLower(),
            ["login"] = Common.Resources.ACCOUNT__LOGIN_LABEL.ToLower(),
            ["password"] = Common.Resources.ACCOUNT__PASSWORD_LABEL.ToLower(),
            ["password_confirm"] = Common.Resources.ACCOUNT__PASSWORD_CONFIRM_LABEL.ToLower(),
        };

        private readonly IDialogService _dialogService = Locator.Current.GetService<IDialogService>()!;
        private readonly IOkBadService _okBadService = Locator.Current.GetService<IOkBadService>()!;

        /// <inheritdoc />
        public async Task<Result<User>> GetUserDataAsync()
        {
            var response = await PassMetaApi.GetAsync<User>("/users/me", true);
            return Result.FromResponse(response);
        }

        /// <inheritdoc />
        public async Task<Result> UpdateUserDataAsync(UserPatchData data)
        {
            if (data.PasswordConfirm is null || data.PasswordConfirm.Length == 0)
            {
                _dialogService.ShowFailure(Common.Resources.WARN__PASSWORD_CONFIRM_MISSED);
                return Result.Failure();
            }

            var response = await PassMetaApi.Patch("/users/me", data).ExecuteAsync<User>();
            if (response is null) 
                return Result.Failure();
            
            if (response.Success)
            {
                _dialogService.ShowInfo(Common.Resources.ACCOUNT__SAVE_SUCCESS);
                
                var user = response.Data;
                await AppConfig.Current.SetUserAsync(user);

                return Result.Success();
            }

            _okBadService.ShowResponseFailure(response, WhatMapper);
            return Result.Failure();
        }
    }
}