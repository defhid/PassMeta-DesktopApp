using System.Threading.Tasks;

using PassMeta.DesktopApp.Common;
using PassMeta.DesktopApp.Common.Abstractions;
using PassMeta.DesktopApp.Common.Abstractions.Services;
using PassMeta.DesktopApp.Common.Abstractions.Utils.PassMetaClient;
using PassMeta.DesktopApp.Common.Models;
using PassMeta.DesktopApp.Common.Models.Entities;
using PassMeta.DesktopApp.Common.Models.Dto.Request;
using PassMeta.DesktopApp.Common.Utils.Mapping;

namespace PassMeta.DesktopApp.Core.Services;

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
    public async Task<IResult> RefreshUserDataAsync()
    {
        var response = await _passMetaClient.Begin(PassMetaApi.User.GetMe())
            .WithBadHandling()
            .ExecuteAsync<User>();

        if (response?.Success is true && AppContext.Current.User?.Equals(response.Data) is not true)
        {
            await AppContext.ApplyAsync(appContext => appContext.User = response.Data);
        }

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

        var response = await _passMetaClient.Begin(PassMetaApi.User.PatchMe())
            .WithJsonBody(data)
            .WithBadMapping(WhatToStringMapper)
            .WithBadHandling()
            .ExecuteAsync<User>();

        if (response?.Success is not true)
        {
            return Result.Failure();
        }
            
        _dialogService.ShowInfo(Resources.ACCOUNT__SAVE_SUCCESS);

        await AppContext.ApplyAsync(appContext => appContext.User = response.Data);

        return Result.Success();
    }
}