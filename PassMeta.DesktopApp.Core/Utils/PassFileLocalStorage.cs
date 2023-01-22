using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using PassMeta.DesktopApp.Common;
using PassMeta.DesktopApp.Common.Abstractions;
using PassMeta.DesktopApp.Common.Abstractions.Services.Logging;
using PassMeta.DesktopApp.Common.Abstractions.UserContext;
using PassMeta.DesktopApp.Common.Abstractions.Utils;
using PassMeta.DesktopApp.Common.Abstractions.Utils.FileRepository;
using PassMeta.DesktopApp.Common.Enums;
using PassMeta.DesktopApp.Common.Extensions;
using PassMeta.DesktopApp.Common.Models;
using PassMeta.DesktopApp.Common.Models.Dto.Internal;
using PassMeta.DesktopApp.Core.Services.Extensions;
using PassMeta.DesktopApp.Core.Utils.Helpers;

namespace PassMeta.DesktopApp.Core.Utils;

/// <inheritdoc />
public class PassFileLocalStorage : IPassFileLocalStorage
{
    private const string EmptyListJson = "[]";

    private readonly IFileRepositoryFactory _repositoryFactory;
    private readonly ILogService _logger;

    /// <summary></summary>
    public PassFileLocalStorage(IFileRepositoryFactory repositoryFactory, ILogService logger)
    {
        _repositoryFactory = repositoryFactory;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<IDetailedResult<IEnumerable<PassFileLocalDto>>> LoadListAsync(
        IUserContext userContext,
        CancellationToken cancellationToken = default)
    {
        _logger.Debug("Loading passfile list...");

        var result = await LoadListInternalAsync(userContext, cancellationToken);
        if (result.Ok)
        {
            _logger.Debug("Passfile list was loaded successfully" );
        }

        return result;
    }

    private async ValueTask<IDetailedResult<List<PassFileLocalDto>>> LoadListInternalAsync(
        IUserContext userContext,
        CancellationToken cancellationToken)
    {
        var repository = _repositoryFactory.ForLocalPassFiles(userContext.UserServerId);

        cancellationToken.ThrowIfCancellationRequested();

        var loadResult = await LoadListJsonAsync(repository, cancellationToken);
        if (loadResult.Bad)
        {
            return loadResult.WithNullData<List<PassFileLocalDto>>();
        }

        cancellationToken.ThrowIfCancellationRequested();

        List<PassFileLocalDto>? passFiles = null;
        try
        {
            passFiles = JsonConvert.DeserializeObject<List<PassFileLocalDto>>(loadResult.Data!) ?? 
                        new List<PassFileLocalDto>();
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Passfile list deserializing failed");

            _ = await MoveInvalidListAsync(repository, cancellationToken);

            var saveResult = await SaveListJsonAsync(EmptyListJson, repository, cancellationToken);
            if (saveResult.Bad)
            {
                return saveResult.WithNullData<List<PassFileLocalDto>>();
            }
        }

        return Result.Success(passFiles ?? new List<PassFileLocalDto>());
    }

    /// <inheritdoc />
    public async Task<IDetailedResult> SaveListAsync(
        IEnumerable<PassFileLocalDto> list,
        IUserContext userContext,
        CancellationToken cancellationToken = default)
    {
        _logger.Debug("Saving passfile list...");
        
        var result = await SaveListInternalAsync(list, userContext, cancellationToken);
        if (result.Ok)
        {
            _logger.Debug("Passfile list was saved successfully");
        }

        return result;
    }

    private async ValueTask<IDetailedResult> SaveListInternalAsync(
        IEnumerable<PassFileLocalDto> list,
        IUserContext userContext,
        CancellationToken cancellationToken)
    {
        var repository = _repositoryFactory.ForLocalPassFiles(userContext.UserServerId);

        cancellationToken.ThrowIfCancellationRequested();

        try
        {
            var listData = JsonConvert.SerializeObject(list);

            cancellationToken.ThrowIfCancellationRequested();

            return await SaveListJsonAsync(listData, repository, cancellationToken);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            return StorageError("Passfile list saving failed", ex);
        }
    }

    /// <inheritdoc />
    public async Task<IDetailedResult<byte[]>> LoadEncryptedContentAsync(
        PassFileType passFileType,
        int passFileId,
        int version,
        IUserContext userContext,
        CancellationToken cancellationToken = default)
    {
        _logger.Debug("Loading passfile #{Id} v{Version} content...", passFileId, version);
        
        var result = await LoadEncryptedContentInternalAsync(
            passFileType, passFileId, version, userContext, cancellationToken);

        if (result.Ok)
        {
            _logger.Debug("Passfile #{Id} v{Version} content was loaded successfully",
                passFileId, version);
        }

        return result;
    }

    private async ValueTask<IDetailedResult<byte[]>> LoadEncryptedContentInternalAsync(
        PassFileType passFileType,
        int passFileId,
        int version,
        IUserContext userContext,
        CancellationToken cancellationToken)
    {
        var repository = _repositoryFactory.ForLocalPassFiles(userContext.UserServerId);
        
        var fileName = PassFilePathHelper.GetPassFileContentName(passFileType, passFileId, version);

        if (!await repository.ExistsAsync(fileName, cancellationToken))
        {
            return Result.Failure<byte[]>(Resources.PASSSTORAGE__VERSION_NOT_FOUND_ERR);
        }

        cancellationToken.ThrowIfCancellationRequested();

        try
        {
            return Result.Success(await repository.ReadAllBytesAsync(fileName, cancellationToken));
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            return StorageError($"Passfile #{passFileId} v{version} content reading failed", ex).WithNullData<byte[]>();
        }
    }

    /// <inheritdoc />
    public async Task<IDetailedResult> SaveEncryptedContentAsync(
        PassFileType passFileType,
        int passFileId,
        int version,
        byte[] content,
        IUserContext userContext,
        CancellationToken cancellationToken = default)
    {
        _logger.Debug("Saving passfile #{Id} v{Version} content...", passFileId, version);

        var result = await SaveEncryptedContentInternalAsync(
            passFileType, passFileId, version, content, userContext, cancellationToken);

        if (result.Ok)
        {
            _logger.Debug("Passfile #{Id} v{Version} content was saved successfully",
                passFileId, version);
        }

        return result;
    }

    private async ValueTask<IDetailedResult> SaveEncryptedContentInternalAsync(
        PassFileType passFileType,
        int passFileId,
        int version,
        byte[] content,
        IUserContext userContext,
        CancellationToken cancellationToken)
    {
        var repository = _repositoryFactory.ForLocalPassFiles(userContext.UserServerId);
        
        var fileName = PassFilePathHelper.GetPassFileContentName(passFileType, passFileId, version);

        cancellationToken.ThrowIfCancellationRequested();

        if (await repository.ExistsAsync(fileName, cancellationToken))
        {
            _logger.Warning($"Passfile #{passFileId} v{version} content will be overwritten!");
        }

        try
        {
            await repository.WriteAllBytesAsync(fileName, content, cancellationToken);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            return StorageError($"Passfile #{passFileId} v{version} content saving failed", ex);
        }

        return Result.Success();
    }

    /// <inheritdoc />
    public async Task<IDetailedResult> DeleteEncryptedContentAsync(
        int passFileId,
        int version,
        IUserContext userContext,
        CancellationToken cancellationToken = default)
    {
        _logger.Debug("Deleting passfile #{Id} v{Version} content...", passFileId, version);

        var result = await DeleteEncryptedContentInternalAsync(passFileId, version, userContext, cancellationToken);
        if (result.Ok)
        {
            _logger.Debug("Passfile #{Id} v{Version} content was deleted successfully", 
                passFileId, version);
        }
        
        return result;
    }

    private async ValueTask<IDetailedResult> DeleteEncryptedContentInternalAsync(
        int passFileId,
        int version,
        IUserContext userContext,
        CancellationToken cancellationToken)
    {
        var repository = _repositoryFactory.ForLocalPassFiles(userContext.UserServerId);

        cancellationToken.ThrowIfCancellationRequested();
        
        try
        {
            var files = await repository.GetFilesAsync(cancellationToken);
            var pattern = PassFilePathHelper.GetPassFileContentNamePattern(passFileId, version);

            cancellationToken.ThrowIfCancellationRequested();

            foreach (var fileName in files.Where(pattern))
            {
                _logger.Debug("Deleting passfile #{Id} v{Version} content '{Path}'...", 
                    passFileId, version, fileName);

                await repository.DeleteAsync(fileName, cancellationToken);
            }
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            return StorageError($"Passfile #{passFileId} v{version} content deleting failed", ex);
        }

        return Result.Success();
    }

    /// <inheritdoc />
    public async Task<IDetailedResult<IEnumerable<int>>> GetVersionsAsync(
        int passFileId,
        IUserContext userContext,
        CancellationToken cancellationToken = default)
    {
        _logger.Debug("Loading passfile #{Id} versions...", passFileId);

        var result = await GetVersionsInternalAsync(passFileId, userContext, cancellationToken);
        if (result.Ok)
        {
            _logger.Debug("Passfile #{Id} versions were loaded successfully", passFileId);
        }

        return result;
    }
    
    private async ValueTask<IDetailedResult<IEnumerable<int>>> GetVersionsInternalAsync(
        int passFileId,
        IUserContext userContext,
        CancellationToken cancellationToken)
    {
        var repository = _repositoryFactory.ForLocalPassFiles(userContext.UserServerId);

        cancellationToken.ThrowIfCancellationRequested();

        List<int> versions;
        try
        {
            var files = await repository.GetFilesAsync(cancellationToken);
            var pattern = PassFilePathHelper.GetPassFileContentNamePattern(passFileId);

            cancellationToken.ThrowIfCancellationRequested();

            versions = files
                .Where(pattern)
                .Select(PassFilePathHelper.GetPassFileVersionFromName)
                .Where(v => v.HasValue)
                .Select(x => x!.Value)
                .ToList();
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            return StorageError($"Passfile #{passFileId} versions loading failed", ex).WithNullData<IEnumerable<int>>();
        }

        return Result.Success(versions);
    }

    #region Others

    private IDetailedResult StorageError(string log, Exception? ex = null)
    {
        log = GetType().Name + ": " + log;
        if (ex is null) _logger.Error(log);
        else _logger.Error(ex, log);
        return Result.Failure(Resources.PASSSTORAGE__ERR);
    }

    private async ValueTask<IDetailedResult<string>> LoadListJsonAsync(IFileRepository repository, CancellationToken cancellationToken)
    {
        const string listFileName = PassFilePathHelper.PassFileListName;

        _logger.Debug("Reading JSON passfile list from '{Path}'...", listFileName);

        if (!await repository.ExistsAsync(listFileName, cancellationToken))
        {
            _logger.Warning("JSON passfile list not found, creating new one...");

            var result = await SaveListJsonAsync(EmptyListJson, repository, cancellationToken);
            if (result.Bad)
            {
                return result.WithNullData<string>();
            }
        }
        
        cancellationToken.ThrowIfCancellationRequested();

        string? listData = null;
        try
        {
            var listBytes = await repository.ReadAllBytesAsync(listFileName, cancellationToken);
            listData = PassFileConvention.JsonEncoding.GetString(listBytes);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.Error(ex, "JSON passfile list reading failed");

            _ = await MoveInvalidListAsync(repository, cancellationToken);

            var result = await SaveListJsonAsync(EmptyListJson, repository, cancellationToken);
            if (result.Bad)
            {
                return result.WithNullData<string>();
            }
        }
        
        _logger.Debug("JSON passfile list was read successfully");

        return Result.Success(listData ?? EmptyListJson);
    }

    private async ValueTask<IDetailedResult> SaveListJsonAsync(string listJson, IFileRepository repository, CancellationToken cancellationToken)
    {
        const string listFileName = PassFilePathHelper.PassFileListName;
        
        _logger.Debug("Saving JSON passfile list to '{Path}'...", listFileName);
        
        cancellationToken.ThrowIfCancellationRequested();

        try
        {
            var listBytes = PassFileConvention.JsonEncoding.GetBytes(listJson);
            await repository.WriteAllBytesAsync(listFileName, listBytes, cancellationToken);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            return StorageError("JSON passfile list saving failed", ex);
        }

        _logger.Debug("JSON passfile list was saved successfully");
        return Result.Success();
    }

    private async ValueTask<IDetailedResult> MoveInvalidListAsync(IFileRepository repository, CancellationToken cancellationToken)
    {
        const string listFileName = PassFilePathHelper.PassFileListName;
        
        if (!await repository.ExistsAsync(listFileName, cancellationToken))
        {
            return Result.Success();
        }

        var invalidListFileName = $"{listFileName}_invalid{DateTime.UtcNow:yyyyMMddhhmmss}";

        _logger.Debug("Renaming invalid JSON passfile list from '{OldName}' to '{NewName}'...", 
            listFileName, invalidListFileName);

        try
        {
            await repository.RenameAsync(listFileName, invalidListFileName, cancellationToken);
        }
        catch (Exception ex)
        {
            return StorageError("Invalid JSON passfile list moving failed", ex);
        }

        _logger.Debug("Invalid JSON passfile list was moved successfully");
        return Result.Success();
    }

    #endregion
}