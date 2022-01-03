namespace PassMeta.DesktopApp.Core.Services
{
    using DesktopApp.Common;
    using DesktopApp.Common.Models;
    using DesktopApp.Common.Models.Entities;
    using DesktopApp.Common.Models.Dto.Request;
    using DesktopApp.Common.Interfaces.Services;
    using DesktopApp.Core.Utils;

    using System.Threading.Tasks;
    using Splat;
    using Utils.Mapping;

    /// <inheritdoc />
    public class AccountService : IAccountService
    {
        /// <summary>
        /// Requests bad mapping.
        /// </summary>
        public static readonly ResourceMapper WhatMapper = new
        (
            ("first_name", () => Resources.ACCOUNT__FIRST_NAME_LABEL.ToLower()),
            ("last_name", () => Resources.ACCOUNT__LAST_NAME_LABEL.ToLower()),
            ("login", () => Resources.ACCOUNT__LOGIN_LABEL.ToLower()),
            ("password", () => Resources.ACCOUNT__PASSWORD_LABEL.ToLower()),
            ("password_confirm", () => Resources.ACCOUNT__PASSWORD_CONFIRM_LABEL.ToLower())
        );

        private readonly IDialogService _dialogService = Locator.Current.GetService<IDialogService>()!;

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
                _dialogService.ShowFailure(Resources.ACCOUNT__PASSWORD_CONFIRM_MISSED_WARN);
                return Result.Failure();
            }

            var response = await PassMetaApi.Patch("/users/me", data)
                .WithBadHandling(WhatMapper)
                .ExecuteAsync<User>();
            
            if (response?.Success is not true)
            {
                return Result.Failure();
            }
            
            _dialogService.ShowInfo(Resources.ACCOUNT__SAVE_SUCCESS);
                
            var user = response.Data;
            await AppConfig.Current.SetUserAsync(user);

            return Result.Success();
        }
    }
}