using System.Threading.Tasks;

using PassMeta.DesktopApp.Common;
using PassMeta.DesktopApp.Common.Abstractions;
using PassMeta.DesktopApp.Common.Abstractions.AppContext;
using PassMeta.DesktopApp.Common.Abstractions.Services;
using PassMeta.DesktopApp.Common.Abstractions.Utils.PassMetaClient;
using PassMeta.DesktopApp.Common.Abstractions.Utils.ValueMapping;
using PassMeta.DesktopApp.Common.Mapping.Values;
using PassMeta.DesktopApp.Common.Models;
using PassMeta.DesktopApp.Common.Models.Entities;
using PassMeta.DesktopApp.Common.Models.Dto.Request;

namespace PassMeta.DesktopApp.Core.Services;

/// <inheritdoc />
public class AccountService : IAccountService
{
    /// <summary>
    /// Requests bad mapping.
    /// </summary>
    public static readonly IValuesMapper<string, string> WhatToStringValuesMapper = UserFieldMapping.FieldToName;

    private readonly IPassMetaClient _passMetaClient;
    private readonly IAppContextManager _appContextManager;
    private readonly IDialogService _dialogService;

    /// <summary></summary>
    public AccountService(
        IPassMetaClient passMetaClient,
        IAppContextManager appContextManager,
        IDialogService dialogService)
    {
        _passMetaClient = passMetaClient;
        _appContextManager = appContextManager;
        _dialogService = dialogService;
    }

    /// <inheritdoc />
    public async Task<IResult> RefreshUserDataAsync()
    {
        var response = await _passMetaClient.Begin(PassMetaApi.User.GetMe())
            .WithBadHandling()
            .ExecuteAsync<User>();

        if (response?.Success is true && _appContextManager.Current.User?.Equals(response.Data) is not true)
        {
            await _appContextManager.ApplyAsync(appContext => appContext.User = response.Data);
        }

        return Result.FromResponse(response);
    }

    /// <inheritdoc />
    public async Task<IResult> UpdateUserDataAsync(UserPatchData data)
    {
        if (data.FullName == _appContextManager.Current.User!.FullName)
        {
            data.FullName = null;
        }

        if (data.Login == _appContextManager.Current.User!.Login)
        {
            data.Login = null;
        }

        var response = await _passMetaClient.Begin(PassMetaApi.User.PatchMe())
            .WithJsonBody(data)
            .WithBadMapping(WhatToStringValuesMapper)
            .WithBadHandling()
            .ExecuteAsync<User>();

        if (response?.Success is not true)
        {
            return Result.Failure();
        }
            
        _dialogService.ShowInfo(Resources.ACCOUNT__SAVE_SUCCESS);

        await _appContextManager.ApplyAsync(appContext => appContext.User = response.Data);

        return Result.Success();
    }
}