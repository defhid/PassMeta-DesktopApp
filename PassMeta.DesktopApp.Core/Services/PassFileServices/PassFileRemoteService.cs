using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using PassMeta.DesktopApp.Common;
using PassMeta.DesktopApp.Common.Abstractions;
using PassMeta.DesktopApp.Common.Abstractions.Services;
using PassMeta.DesktopApp.Common.Abstractions.Services.PassFileServices;
using PassMeta.DesktopApp.Common.Abstractions.Utils.Logging;
using PassMeta.DesktopApp.Common.Abstractions.Utils.PassMetaClient;
using PassMeta.DesktopApp.Common.Conventions;
using PassMeta.DesktopApp.Common.Models;
using PassMeta.DesktopApp.Common.Models.Dto.Request;
using PassMeta.DesktopApp.Common.Models.Dto.Response;
using PassMeta.DesktopApp.Common.Models.Dto.Response.OkBad;
using PassMeta.DesktopApp.Common.Models.Entities.PassFile;
using PassMeta.DesktopApp.Common.Utils.ValueMapping;
using PassMeta.DesktopApp.Core.Extensions;
using PassMeta.DesktopApp.Core.Services.Extensions;

namespace PassMeta.DesktopApp.Core.Services.PassFileServices;

/// <inheritdoc />
public class PassFileRemoteService : IPassFileRemoteService
{
    private static readonly ValuesMapper<string, string> WhatToStringValuesMapper = new MapToResource<string>[]
    {
        new("passfile_id", () => Resources.DICT_STORAGE__PASSFILE_ID),
        new("name", () => Resources.DICT_STORAGE__PASSFILE_NAME),
        new("color", () => Resources.DICT_STORAGE__PASSFILE_COLOR),
        new("created_on", () => Resources.DICT_STORAGE__PASSFILE_CREATED_ON),
        new("check_password", () => Resources.DICT_STORAGE__CHECK_PASSWORD)
    };

    private readonly IPassMetaClient _pmClient;
    private readonly IPassFileCryptoService _pfCryptoService;
    private readonly IMapper _mapper;
    private readonly IDialogService _dialogService;
    private readonly ILogsWriter _logger;

    /// <summary></summary>
    public PassFileRemoteService(
        IPassMetaClient pmClient,
        IPassFileCryptoService pfCryptoService,
        IMapper mapper,
        IDialogService dialogService,
        ILogsWriter logger)
    {
        _pmClient = pmClient;
        _pfCryptoService = pfCryptoService;
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
    public async Task<IResult<TPassFile>> SaveEncryptedContentAsync<TPassFile, TContent>(TPassFile passFile)
        where TPassFile : PassFile<TContent>
        where TContent : class, new()
    {
        if (passFile.Content.Encrypted is null)
        {
            var result = _pfCryptoService.Encrypt(passFile);
            if (result.Bad)
            {
                _dialogService.ShowError(result.Message!);
                return Result.Failure<TPassFile>();
            }
        }

        var request = _pmClient.Begin(PassMetaApi.PassFile.PostVersion(passFile.Id))
            .WithFormBody(new { smth = passFile.Content.Encrypted })
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
    public async Task<IResult> DeleteAsync(PassFile passFile, string accountPassword)
    {
        var request = _pmClient.Begin(PassMetaApi.PassFile.Delete(passFile.Id))
            .WithJsonBody(new PassFileDeleteData { CheckPassword = accountPassword })
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