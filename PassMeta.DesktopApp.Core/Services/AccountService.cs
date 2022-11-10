namespace PassMeta.DesktopApp.Core.Services
{
    using DesktopApp.Common;
    using DesktopApp.Common.Models;
    using DesktopApp.Common.Models.Entities;
    using DesktopApp.Common.Models.Dto.Request;

    using System.Threading.Tasks;
    using Common.Abstractions;
    using Common.Abstractions.Services;
    using Common.Abstractions.Utils;
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

        private readonly IPassMetaClient _passMetaClient;
        private readonly IDialogService _dialogService;

        /// <summary></summary>
        public AccountService(IPassMetaClient passMetaClient, IDialogService dialogService)
        {
            _passMetaClient = passMetaClient;
            _dialogService = dialogService;
        }

        /// <inheritdoc />
        public async Task<IResult<User>> GetUserDataAsync()
        {
            var response = await _passMetaClient.Get(PassMetaApi.User.GetMe())
                .WithBadHandling()
                .ExecuteAsync<User>();

            return Result.FromResponse(response);
        }

        /// <inheritdoc />
        public async Task<IResult> UpdateUserDataAsync(UserPatchData data)
        {
            if (data.FullName == AppContext.Current.User!.FullName)
            {
                data.FullName = null;
            }

            if (data.Login == AppContext.Current.User!.Login)
            {
                data.Login = null;
            }

            var response = await _passMetaClient.Patch(PassMetaApi.User.PatchMe())
                .WithJsonBody(data)
                .WithBadMapping(WhatToStringMapper)
                .WithBadHandling()
                .ExecuteAsync<User>();

            if (response?.Success is not true)
            {
                return Result.Failure();
            }
            
            _dialogService.ShowInfo(Resources.ACCOUNT__SAVE_SUCCESS);
                
            AppContext.Current.User = response.Data!;
            await AppContext.FlushCurrentAsync();

            return Result.Success();
        }
    }
}