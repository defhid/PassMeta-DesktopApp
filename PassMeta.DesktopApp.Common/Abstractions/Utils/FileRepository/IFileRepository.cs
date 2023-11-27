using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace PassMeta.DesktopApp.Common.Abstractions.Utils.FileRepository;

/// <summary>
/// File repository with only one directory.
/// </summary>
/// <remarks>Singleton.</remarks>
public interface IFileRepository
{
    /// <summary>
    /// Get absolute file path by its name.
    /// </summary>
    string GetAbsolutePath(string? relativePath);

    /// <summary>
    /// Get all repository file names.
    /// </summary>
    ValueTask<IEnumerable<string>> GetFilesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Does a file with given name exists.
    /// </summary>
    ValueTask<bool> ExistsAsync(string fileName, CancellationToken cancellationToken = default);

    /// <summary>
    /// Read all bytes from the file with given name.
    /// </summary>
    ValueTask<byte[]> ReadAllBytesAsync(string fileName, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Write all bytes to the file with given name.
    /// </summary>
    ValueTask WriteAllBytesAsync(string fileName, byte[] bytes, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Delete the file with given name.
    /// </summary>
    ValueTask DeleteAsync(string fileName, CancellationToken cancellationToken = default);

    /// <summary>
    /// Rename the file with given name.
    /// </summary>
    ValueTask RenameAsync(string fileName, string actualFileName, CancellationToken cancellationToken = default);
}