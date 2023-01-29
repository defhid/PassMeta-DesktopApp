using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using PassMeta.DesktopApp.Common.Models.Entities.PassFile;

namespace PassMeta.DesktopApp.Common.Abstractions.Services.PassFileServices;

/// <summary>
/// Service for working with remote passfiles.
/// </summary>
public interface IPassFileRemoteService
{
    /// <summary>
    /// Load actual user's passfile list of specified type.
    /// </summary>
    Task<IResult<IEnumerable<TPassFile>>> GetListAsync<TPassFile>(CancellationToken cancellationToken = default)
        where TPassFile : PassFile;

    /// <summary>
    /// Load actual passfile information from the server.
    /// </summary>
    Task<IResult<TPassFile>> GetInfoAsync<TPassFile>(TPassFile passFile, CancellationToken cancellationToken = default)
        where TPassFile : PassFile;

    /// <summary>
    /// Load passfile encrypted content of specified version.
    /// </summary>
    Task<IResult<byte[]>> GetEncryptedContentAsync(
        long passFileId,
        int version,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Add a new passfile and get actual information.
    /// </summary>
    Task<IResult<TPassFile>> AddAsync<TPassFile>(TPassFile passFile)
        where TPassFile : PassFile;

    /// <summary>
    /// Save passfile information and get actual.
    /// </summary>
    Task<IResult<PassFile>> SaveInfoAsync<TPassFile>(TPassFile passFile)
        where TPassFile : PassFile;

    /// <summary>
    /// Save passfile content and get actual information.
    /// </summary>
    Task<IResult<TPassFile>> SaveEncryptedContentAsync<TPassFile, TContent>(TPassFile passFile)
        where TPassFile : PassFile<TContent>
        where TContent : class, new();

    /// <summary>
    /// Delete passfile.
    /// </summary>
    Task<IResult> DeleteAsync(PassFile passFile, string accountPassword);
}