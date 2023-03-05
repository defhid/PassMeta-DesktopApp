using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using PassMeta.DesktopApp.Common.Abstractions.App;
using PassMeta.DesktopApp.Common.Enums;
using PassMeta.DesktopApp.Common.Models.Dto.Internal;

namespace PassMeta.DesktopApp.Common.Abstractions.Utils;

/// <summary>
/// Local passfile storage.
/// </summary>
/// <remarks>Singleton.</remarks>
public interface IPassFileLocalStorage
{
    /// <summary>
    /// Load actual list of local passfiles.
    /// </summary>
    Task<IDetailedResult<IEnumerable<PassFileLocalDto>>> LoadListAsync(
        IUserContext userContext,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Save actual list of local passfiles.
    /// </summary>
    Task<IDetailedResult> SaveListAsync(
        IEnumerable<PassFileLocalDto> list,
        IUserContext userContext,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Load local encrypted content by passfile id and version.
    /// </summary>
    Task<IDetailedResult<byte[]>> LoadEncryptedContentAsync(
        PassFileType passFileType,
        long passFileId,
        int version,
        IUserContext userContext,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Save local encrypted content by passfile id and version.
    /// </summary>
    Task<IDetailedResult> SaveEncryptedContentAsync(
        PassFileType passFileType,
        long passFileId,
        int version,
        byte[] content,
        IUserContext userContext,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Delete local encrypted content by passfile id and version.
    /// </summary>
    Task<IDetailedResult> DeleteEncryptedContentAsync(
        long passFileId,
        int version,
        IUserContext userContext,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get all locally saved versions of encrypted content by passfile id.
    /// </summary>
    Task<IDetailedResult<IEnumerable<int>>> GetVersionsAsync(
        long passFileId,
        IUserContext userContext,
        CancellationToken cancellationToken = default);
}