using System;
using System.Net;
using System.Threading.Tasks;
using PassMeta.DesktopApp.Common;
using PassMeta.DesktopApp.Common.Abstractions;
using PassMeta.DesktopApp.Common.Abstractions.App;
using PassMeta.DesktopApp.Common.Abstractions.Services;
using PassMeta.DesktopApp.Common.Abstractions.Utils.PassMetaClient;
using PassMeta.DesktopApp.Common.Abstractions.Utils.ValueMapping;
using PassMeta.DesktopApp.Common.Mapping.Values;
using PassMeta.DesktopApp.Common.Models;
using PassMeta.DesktopApp.Common.Models.Dto.Request;
using PassMeta.DesktopApp.Common.Models.Entities;

namespace PassMeta.DesktopApp.Core.Services;

/// <inheritdoc />
public class AuthService : IAuthService
{
    private static readonly IValuesMapper<string, string> WhatToStringValuesMapper = UserFieldMapping.FieldToName;
        
    private readonly IPassMetaClient _passMetaClient;
    private readonly IAppContextManager _appContextManager;
    private readonly IDialogService _dialogService;

    /// <summary></summary>
    public AuthService(
        IPassMetaClient passMetaClient,
        IAppContextManager appContextManager,
        IDialogService dialogService)
    {
        _passMetaClient = passMetaClient;
        _appContextManager = appContextManager;
        _dialogService = dialogService;
    }

    /// <inheritdoc />
    public async Task<IResult<User>> LogInAsync(SignInPostData data)
    {
        if (!_Validate(data).Ok)
        {
            _dialogService.ShowError(Resources.AUTH__DATA_VALIDATION_ERR);
            return Result.Failure<User>();
        }
            
        var response = await _passMetaClient.Begin(PassMetaApi.Auth.PostLogIn())
            .WithJsonBody(data)
            .WithBadMapping(WhatToStringValuesMapper)
            .WithBadHandling()
            .ExecuteAsync<User>();
            
        if (response?.Success is not true)
            return Result.Failure<User>();

        await _appContextManager.ApplyAsync(appContext => appContext.User = response.Data);

        return Result.Success(response.Data!);
    }

    /// <inheritdoc />
    public async Task LogOutAsync()
    {
        var answer = await _dialogService.ConfirmAsync(Resources.ACCOUNT__SIGN_OUT_CONFIRM);
        if (answer.Bad) return;

        await _appContextManager.ApplyAsync(appContext =>
        {
            appContext.User = null;
            appContext.Cookies = Array.Empty<Cookie>();
        });
    }

    /// <inheritdoc />
    public async Task ResetAllExceptMeAsync()
    {
        var answer = await _dialogService.ConfirmAsync(Resources.ACCOUNT__RESET_SESSIONS_CONFIRM);
        if (answer.Bad) return;

        var response = await _passMetaClient.Begin(PassMetaApi.Auth.PostResetAllExceptMe())
            .WithBadHandling()
            .ExecuteAsync();

        if (response?.Success is true)
        {
            _dialogService.ShowInfo(Resources.ACCOUNT__RESET_SESSIONS_SUCCESS);
        }
    }

    /// <inheritdoc />
    public async Task<IResult> RegisterAsync(SignUpPostData data)
    {
        if (!_Validate(data).Ok)
        {
            _dialogService.ShowError(Resources.AUTH__DATA_VALIDATION_ERR);
            return Result.Failure<User>();
        }
            
        var response = await _passMetaClient.Begin(PassMetaApi.User.Post())
            .WithJsonBody(data)
            .WithBadMapping(WhatToStringValuesMapper)
            .WithBadHandling()
            .ExecuteAsync<User>();
            
        if (response?.Success is not true) 
            return Result.Failure<User>();

        await _appContextManager.ApplyAsync(appContext => appContext.User = response.Data);

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