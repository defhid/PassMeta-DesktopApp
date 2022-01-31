namespace PassMeta.DesktopApp.Core.Services
{
    using DesktopApp.Common;
    using DesktopApp.Common.Models;
    using DesktopApp.Common.Models.Entities;
    using DesktopApp.Common.Models.Dto.Request;
    using DesktopApp.Common.Interfaces.Services;
    using DesktopApp.Core.Utils;

    using System.Threading.Tasks;
    using Common.Utils.Mapping;

    /// <inheritdoc />
    public class AccountService : IAccountService
    {
        /// <summary>
        /// Requests bad mapping.
        /// </summary>
        public static readonly SimpleMapper<string, string> WhatToStringMapper = new MapToResource<string>[]
        {
            new("first_name", () => Resources.DICT_ACCOUNT__FIRST_NAME),
            new("last_name", () => Resources.DICT_ACCOUNT__LAST_NAME),
            new("login", () => Resources.DICT_ACCOUNT__LOGIN),
            new("password", () => Resources.DICT_ACCOUNT__PASSWORD),
            new("password_confirm", () => Resources.DICT_ACCOUNT__PASSWORD_CONFIRM)
        };

        private readonly IDialogService _dialogService = EnvironmentContainer.Resolve<IDialogService>();

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
                .WithBadMapping(WhatToStringMapper)
                .WithBadHandling()
                .ExecuteAsync<User>();
            
            if (response?.Success is not true)
            {
                return Result.Failure();
            }
            
            _dialogService.ShowInfo(Resources.ACCOUNT__SAVE_SUCCESS);
                
            var user = response.Data!;
            await AppContext.SetUserAsync(user);

            return Result.Success();
        }
    }
}