using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using PassMeta.DesktopApp.Common;
using PassMeta.DesktopApp.Common.Abstractions;
using PassMeta.DesktopApp.Common.Abstractions.Services;
using PassMeta.DesktopApp.Common.Abstractions.Services.PassFileServices;
using PassMeta.DesktopApp.Common.Abstractions.Utils.Helpers;
using PassMeta.DesktopApp.Common.Abstractions.Utils.Logging;
using PassMeta.DesktopApp.Common.Abstractions.Utils.PassMetaClient;
using PassMeta.DesktopApp.Common.Abstractions.Utils.ValueMapping;
using PassMeta.DesktopApp.Common.Conventions;
using PassMeta.DesktopApp.Common.Extensions;
using PassMeta.DesktopApp.Common.Mapping.Values;
using PassMeta.DesktopApp.Common.Models;
using PassMeta.DesktopApp.Common.Models.Dto.Request;
using PassMeta.DesktopApp.Common.Models.Dto.Response;
using PassMeta.DesktopApp.Common.Models.Dto.Response.OkBad;
using PassMeta.DesktopApp.Common.Models.Entities.PassFile;

namespace PassMeta.DesktopApp.Core.Services.PassFileServices;

/// <inheritdoc />
public class PassFileRemoteService : IPassFileRemoteService
{
    private static readonly IValuesMapper<string, string> WhatToStringValuesMapper = PassFileFieldMapping.FieldToName;
    private readonly IPassMetaClient _pmClient;
    private readonly IMapper _mapper;
    private readonly IDialogService _dialogService;
    private readonly ILogsWriter _logger;

    /// <summary></summary>
    public PassFileRemoteService(
        IPassMetaClient pmClient,
        IMapper mapper,
        IDialogService dialogService,
        ILogsWriter logger)
    {
        _pmClient = pmClient;
        _mapper = mapper;
        _dialogService = dialogService;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<IResult<IEnumerable<TPassFile>>> GetListAsync<TPassFile>(
        CancellationToken cancellationToken = default)
        where TPassFile : PassFile
    {
        var passFileType = PassFileConvention.GetPassFileType<TPassFile>();

        var response = await _pmClient.Begin(PassMetaApi.PassFile.GetList(passFileType))
            .WithBadHandling()
            .ExecuteAsync<List<PassFileInfoDto>>(cancellationToken);

        _logger.Debug("PasFile list was fetched from the server: {Success}",
            GetIsSuccess(response));

        return response?.Success is true
            ? Result.Success(response.Data!.Select(_mapper.Map<PassFileInfoDto, TPassFile>))
            : Result.Failure<IEnumerable<TPassFile>>();
    }

    /// <inheritdoc />
    public async Task<IResult<TPassFile>> GetInfoAsync<TPassFile>(
        TPassFile passFile,
        CancellationToken cancellationToken = default)
        where TPassFile : PassFile
    {
        var response = await _pmClient.Begin(PassMetaApi.PassFile.Get(passFile.Id))
            .WithContext(passFile.GetTitle())
            .WithBadHandling()
            .ExecuteAsync<PassFileInfoDto>(cancellationToken);

        _logger.Debug("PasFile #{Id} info was fetched from the server: {Success}",
            passFile.Id, GetIsSuccess(response));

        return response?.Success is true
            ? Result.Success(_mapper.Map<PassFileInfoDto, TPassFile>(response.Data!))
            : Result.Failure<TPassFile>();
    }

    /// <inheritdoc />
    public async Task<IResult<IEnumerable<PassFileVersionDto>>> GetVersionsAsync(long passFileId, CancellationToken cancellationToken = default)
    {
        var response = await _pmClient.Begin(PassMetaApi.PassFile.GetVersionList(passFileId))
            .WithBadHandling()
            .ExecuteAsync<List<PassFileVersionDto>>(cancellationToken);

        _logger.Debug("PasFile #{Id} version list was fetched from the server: {Success}",
            passFileId, GetIsSuccess(response));

        return response?.Success is true
            ? Result.Success(response.Data!)
            : Result.Failure<IEnumerable<PassFileVersionDto>>();
    }

    /// <inheritdoc />
    public async Task<IResult<byte[]>> GetEncryptedContentAsync(
        long passFileId,
        int version,
        CancellationToken cancellationToken = default)
    {
        var data = await _pmClient.Begin(PassMetaApi.PassFile.GetVersion(passFileId, version))
            .WithBadHandling()
            .ExecuteRawAsync(cancellationToken);

        _logger.Debug("PasFile #{Id} v{Version} content was fetched from the server: {Success}",
            passFileId, version, GetIsSuccess(data is not null));

        return data is not null
            ? Result.Success(data)
            : Result.Failure<byte[]>();
    }

    /// <inheritdoc />
    public async Task<IResult<TPassFile>> AddAsync<TPassFile>(TPassFile passFile)
        where TPassFile : PassFile
    {
        var response = await _pmClient.Begin(PassMetaApi.PassFile.Post())
            .WithJsonBody(_mapper.Map<PassFile, PassFilePostData>(passFile))
            .WithContext(passFile.GetTitle())
            .WithBadMapping(WhatToStringValuesMapper)
            .WithBadHandling()
            .ExecuteAsync<PassFileInfoDto>();

        _logger.Debug("PassFile #{Id} info was added to the server: {Success}",
            passFile.Id, GetIsSuccess(response));

        return response?.Success is true 
            ? Result.Success(_mapper.Map<PassFileInfoDto, TPassFile>(response.Data!)) 
            : Result.Failure<TPassFile>();
    }

    /// <inheritdoc />
    public async Task<IResult<PassFile>> SaveInfoAsync<TPassFile>(TPassFile passFile)
        where TPassFile : PassFile
    {
        var request = _pmClient.Begin(PassMetaApi.PassFile.Patch(passFile.Id))
            .WithJsonBody(new PassFileInfoPatchData
            {
                Name = passFile.Name,
                Color = passFile.Color
            })
            .WithContext(passFile.GetTitle())
            .WithBadMapping(WhatToStringValuesMapper)
            .WithBadHandling();

        var response = await request.ExecuteAsync<PassFileInfoDto>();

        _logger.Debug("PassFile #{Id} info was saved on the server: {Success}",
            passFile.Id, GetIsSuccess(response));

        return response?.Success is true 
            ? Result.Success(_mapper.Map<PassFileInfoDto, TPassFile>(response.Data!)) 
            : Result.Failure<TPassFile>();
    }

    /// <inheritdoc />
    public async Task<IResult<TPassFile>> SaveEncryptedContentAsync<TPassFile>(TPassFile passFile)
        where TPassFile : PassFile
    {
        if (passFile.ContentEncrypted is null)
        {
            _logger.Error($"Saving passfile #{passFile.Id} content failed because of null encrypted content!");
            _dialogService.ShowError(Resources.PASSERVICE__ERR);
            return Result.Failure<TPassFile>();
        }

        var request = _pmClient.Begin(PassMetaApi.PassFile.PostVersion(passFile.Id))
            .WithFormBody(new { smth = passFile.ContentEncrypted })
            .WithContext(passFile.GetTitle())
            .WithBadMapping(WhatToStringValuesMapper)
            .WithBadHandling();

        var response = await request.ExecuteAsync<PassFileInfoDto>();

        _logger.Debug("PassFile #{Id} content was saved on the server: {Success}",
            passFile.Id, GetIsSuccess(response));

        return response?.Success is true
            ? Result.Success(_mapper.Map<PassFileInfoDto, TPassFile>(response.Data!))
            : Result.Failure<TPassFile>();
    }

    /// <inheritdoc />
    public async Task<IResult> DeleteAsync(PassFile passFile)
    {
        var request = _pmClient.Begin(PassMetaApi.PassFile.Delete(passFile.Id))
            .WithContext(passFile.GetTitle())
            .WithBadMapping(WhatToStringValuesMapper)
            .WithBadHandling();

        var response = await request.ExecuteAsync();

        _logger.Debug("PassFile #{Id} was deleted from the server: {Success}",
            passFile.Id, GetIsSuccess(response));

        return Result.FromResponse(response);
    }

    private static string GetIsSuccess(OkBadResponse? response) => GetIsSuccess(response?.Success is true);
    private static string GetIsSuccess(bool success) => success ? "SUCCESS" : "FAILURE";
}