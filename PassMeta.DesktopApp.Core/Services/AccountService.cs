using System.Collections.Generic;
using System.Threading.Tasks;
using PassMeta.DesktopApp.Common.Interfaces.Services;
using PassMeta.DesktopApp.Common.Models;
using PassMeta.DesktopApp.Common.Models.Entities;
using PassMeta.DesktopApp.Common.Models.Entities.Request;
using PassMeta.DesktopApp.Core.Utils;
using Splat;

namespace PassMeta.DesktopApp.Core.Services
{
    public class AccountService : IAccountService
    {
        private readonly Dictionary<string, string> _whatMapper = new()
        {
            ["first_name"] = Common.Resources.ACCOUNT__FIRST_NAME_LABEL.ToLower(),
            ["last_name"] = Common.Resources.ACCOUNT__LAST_NAME_LABEL.ToLower(),
            ["login"] = Common.Resources.ACCOUNT__LOGIN_LABEL.ToLower(),
            ["password"] = Common.Resources.ACCOUNT__PASSWORD_LABEL.ToLower(),
            ["password_confirm"] = Common.Resources.ACCOUNT__PASSWORD_CONFIRM_LABEL.ToLower(),
        };

        public async Task<Result<User>> GetUserDataAsync()
        {
            var response = await PassMetaApi.GetAsync<User>("/users/me", true);
            return response?.ToResult() ?? new Result<User>(false);
        }

        public async Task<Result> UpdateUserDataAsync(UserPatchData data)
        {
            if (data.PasswordConfirm is null || data.PasswordConfirm.Length == 0)
            {
                Locator.Current.GetService<IDialogService>()!.ShowFailure(Common.Resources.WARN__PASSWORD_CONFIRM_MISSED);
                return Result.Failure;
            }

            var response = await PassMetaApi.PatchAsync<UserPatchData, User>("/users/me", data, true);
            if (response is null) 
                return Result.Failure;
            
            if (response.Success)
            {
                Locator.Current.GetService<IDialogService>()!.ShowInfo(Common.Resources.ACCOUNT__SAVE_SUCCESS);
                
                var user = response.Data;
                await AppConfig.Current.SetUserAsync(user);

                return Result.Success;
            }

            Locator.Current.GetService<IOkBadService>()!.ShowResponseFailure(response, _whatMapper);
            return Result.Failure;
        }
    }
}