using System.Collections.Generic;
using System.Threading.Tasks;
using PassMeta.DesktopApp.Common.Enums;
using PassMeta.DesktopApp.Common.Models.Dto.Response.OkBad;
using PassMeta.DesktopApp.Common.Models.Entities.PassFile;

namespace PassMeta.DesktopApp.Common.Abstractions.Services.PassFileServices;

/// <summary>
/// Service for working with remote passfiles.
/// </summary>
public interface IPassFileRemoteService
{
    /// <summary>
    /// Load user's passfile list of specified type without encrypted data.
    /// </summary>
    Task<List<PassFile>?> GetListAsync(PassFileType ofType);

    /// <summary>
    /// Load passfile with encrypted data from the server.
    /// </summary>
    Task<IResult<PassFile>> GetInfoAsync(int passFileId);

    /// <summary>
    /// Load passfile encrypted data.
    /// </summary>
    Task<byte[]?> GetEncryptedContentAsync(int passFileId, int version);
    
    /// <summary>
    /// Add a new passfile.
    /// </summary>
    Task<IResult<PassFile>> AddAsync(PassFile passFile);

    /// <summary>
    /// Save passfile information fields.
    /// </summary>
    Task<OkBadResponse<PassFile>?> SaveInfoAsync(PassFile passFile);

    /// <summary>
    /// Save passfile data fields with encrypted data.
    /// </summary>
    Task<OkBadResponse<PassFile>?> SaveEncryptedContentAsync(int passFileId, byte[] bytes);

    /// <summary>
    /// Delete passfile.
    /// </summary>
    Task<OkBadResponse?> DeleteAsync(int passFileId, string accountPassword);
}