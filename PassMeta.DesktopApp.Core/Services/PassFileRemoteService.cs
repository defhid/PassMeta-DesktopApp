using System.Collections.Generic;
using System.Threading.Tasks;

using PassMeta.DesktopApp.Common;
using PassMeta.DesktopApp.Common.Abstractions;
using PassMeta.DesktopApp.Common.Abstractions.Services.PassFile;
using PassMeta.DesktopApp.Common.Abstractions.Services.Logging;
using PassMeta.DesktopApp.Common.Abstractions.Utils.PassMetaClient;
using PassMeta.DesktopApp.Common.Enums;
using PassMeta.DesktopApp.Common.Models;
using PassMeta.DesktopApp.Common.Models.Dto.Request;
using PassMeta.DesktopApp.Common.Models.Entities;
using PassMeta.DesktopApp.Common.Utils.Mapping;

using PassMeta.DesktopApp.Core.Services.Extensions;
using PassMeta.DesktopApp.Core.Utils.Extensions;

namespace PassMeta.DesktopApp.Core.Services;

/// <inheritdoc />
public class PassFileRemoteService : IPassFileRemoteService
{
    private static readonly SimpleMapper<string, string> WhatToStringMapper = new MapToResource<string>[]
    {
        new("passfile_id", () => Resources.DICT_STORAGE__PASSFILE_ID),
        new("name", () => Resources.DICT_STORAGE__PASSFILE_NAME),
        new("color", () => Resources.DICT_STORAGE__PASSFILE_COLOR),
        new("created_on", () => Resources.DICT_STORAGE__PASSFILE_CREATED_ON),
        new("smth", () => Resources.DICT_STORAGE__PASSFILE_SMTH),
        new("check_password", () => Resources.DICT_STORAGE__CHECK_PASSWORD)
    };

    private readonly IPassMetaClient _passMetaClient;
    private readonly ILogService _logger;

    /// <summary></summary>
    public PassFileRemoteService(IPassMetaClient passMetaClient, ILogService logger)
    {
        _passMetaClient = passMetaClient;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<IResult<PassFile>> GetInfoAsync(int passFileId)
    {
        var response = await _passMetaClient.Begin(PassMetaApi.PassFile.Get(passFileId))
            .WithBadHandling()
            .ExecuteAsync<PassFile>();

        _logger.Info($"PasFile #{passFileId} info was fetched from the server: " + GetIsSuccess(response));

        return Result.FromResponse(response);
    }
        
    /// <inheritdoc />
    public async Task<List<PassFile>?> GetListAsync(PassFileType ofType)
    {
        var response = await _passMetaClient.Begin(PassMetaApi.PassFile.GetList(ofType))
            .WithBadHandling()
            .ExecuteAsync<List<PassFile>>();

        _logger.Info("PasFile list was fetched from the server: " + GetIsSuccess(response));
            
        return response?.Data;
    }

    /// <inheritdoc />
    public async Task<byte[]?> GetDataAsync(int passFileId, int version)
    {
        var data =  await _passMetaClient.Begin(PassMetaApi.PassFile.GetVersion(passFileId, version))
            .WithBadHandling()
            .ExecuteRawAsync();

        _logger.Info($"PasFile #{passFileId} v{version} content was fetched from the server: " + GetIsSuccess(data is not null));

        return data;
    }

    /// <inheritdoc />
    public async Task<OkBadResponse<PassFile>?> SaveInfoAsync(PassFile passFile)
    {
        var request = _passMetaClient.Begin(PassMetaApi.PassFile.Patch(passFile.Id))
            .WithJsonBody(new PassFileInfoPatchData
            {
                Name = passFile.Name,
                Color = passFile.Color
            })
            .WithContext(passFile.GetTitle())
            .WithBadMapping(WhatToStringMapper)
            .WithBadHandling();

        var response = await request.ExecuteAsync<PassFile>();

        _logger.Info($"PassFile #{passFile.Id} info was saved on the server: " + GetIsSuccess(response));
            
        return response;
    }

    /// <inheritdoc />
    public async Task<OkBadResponse<PassFile>?> SaveDataAsync(PassFile passFile)
    {
        var request = _passMetaClient.Begin(PassMetaApi.PassFile.PostVersion(passFile.Id))
            .WithFormBody(new
            {
                smth = passFile.DataEncrypted,
            })
            .WithContext(passFile.GetTitle())
            .WithBadMapping(WhatToStringMapper)
            .WithBadHandling();

        var response = await request.ExecuteAsync<PassFile>();

        _logger.Info($"PassFile #{passFile.Id} content was saved on the server: " + GetIsSuccess(response));

        return response;
    }

    /// <inheritdoc />
    public async Task<IResult<PassFile>> AddAsync(PassFile passFile)
    {
        var infoResponse = await _passMetaClient.Begin(PassMetaApi.PassFile.Post())
            .WithJsonBody(new PassFilePostData
            {
                Name = passFile.Name,
                Color = passFile.Color,
                TypeId = passFile.TypeId,
                CreatedOn = passFile.CreatedOn,
            })
            .WithContext(passFile.GetTitle())
            .WithBadMapping(WhatToStringMapper)
            .WithBadHandling()
            .ExecuteAsync<PassFile>();

        _logger.Info($"PassFile #{passFile.Id} info was added to the server: " + GetIsSuccess(infoResponse));

        if (infoResponse?.Success is not true)
        {
            return Result.Failure<PassFile>();
        }

        var actualPassFile = infoResponse.Data!;
        actualPassFile.DataEncrypted = passFile.DataEncrypted;

        var dataResponse = await SaveDataAsync(actualPassFile);
        if (dataResponse?.Success is true)
        {
            actualPassFile.RefreshDataFieldsFrom(dataResponse.Data!.WithEncryptedDataFrom(actualPassFile), false);
        }

        return Result.Success(actualPassFile);
    }

    /// <inheritdoc />
    public async Task<OkBadResponse?> DeleteAsync(PassFile passFile, string accountPassword)
    {
        var request = _passMetaClient.Begin(PassMetaApi.PassFile.Delete(passFile.Id))
            .WithJsonBody(new PassFileDeleteData
            {
                CheckPassword = accountPassword
            })
            .WithContext(passFile.GetTitle())
            .WithBadMapping(WhatToStringMapper)
            .WithBadHandling();

        var response = await request.ExecuteAsync();

        _logger.Info($"PassFile #{passFile.Id} was deleted from the server: " + GetIsSuccess(response));

        return response;
    }

    private static string GetIsSuccess(OkBadResponse? response) => GetIsSuccess(response?.Success is true);
    private static string GetIsSuccess(bool success) => success ? "SUCCESS" : "FAILURE";
}