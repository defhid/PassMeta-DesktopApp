using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using PassMeta.DesktopApp.Common.Models.Dto.Response;
using PassMeta.DesktopApp.Common.Models.Entities.PassFile;

namespace PassMeta.DesktopApp.Common.Abstractions.Services.PassFileServices;

/// <summary>
/// Service for working with remote passfiles.
/// </summary>
public interface IPassFileRemoteService
{
    /// <summary>
    /// Load actual user's passfile list of specified type from remote.
    /// </summary>
    Task<IResult<IEnumerable<TPassFile>>> GetListAsync<TPassFile>(CancellationToken cancellationToken = default)
        where TPassFile : PassFile;

    /// <summary>
    /// Load actual passfile information from remote.
    /// </summary>
    Task<IResult<TPassFile>> GetInfoAsync<TPassFile>(TPassFile passFile, CancellationToken cancellationToken = default)
        where TPassFile : PassFile;

    /// <summary>
    /// Get all remote versions of encrypted content by passfile id.
    /// </summary>
    Task<IResult<IEnumerable<PassFileVersionDto>>> GetVersionsAsync(long passFileId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Load passfile encrypted content of specified version from remote.
    /// </summary>
    Task<IResult<byte[]>> GetEncryptedContentAsync(
        long passFileId,
        int version,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Add a new passfile to remote and get actual information.
    /// </summary>
    Task<IResult<TPassFile>> AddAsync<TPassFile>(TPassFile passFile)
        where TPassFile : PassFile;

    /// <summary>
    /// Save passfile information to remote and get actual.
    /// </summary>
    Task<IResult<PassFile>> SaveInfoAsync<TPassFile>(TPassFile passFile)
        where TPassFile : PassFile;

    /// <summary>
    /// Save passfile content to remote and get actual information.
    /// </summary>
    /// <remarks><see cref="PassFile.ContentEncrypted"/> must not be null!</remarks>
    Task<IResult<TPassFile>> SaveEncryptedContentAsync<TPassFile>(TPassFile passFile)
        where TPassFile : PassFile;

    /// <summary>
    /// Delete passfile from remote.
    /// </summary>
    Task<IResult> DeleteAsync(PassFile passFile);
}