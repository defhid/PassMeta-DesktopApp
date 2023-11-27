using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using PassMeta.DesktopApp.Common.Abstractions.Utils.FileRepository;

namespace PassMeta.DesktopApp.Core.Utils.FileRepository;

/// <inheritdoc />
public class LocalFileRepository : IFileRepository
{
    private readonly string _rootPath;

    /// <summary></summary>
    public LocalFileRepository(string rootPath)
    {
        _rootPath = rootPath;
    }

    /// <inheritdoc />
    public string GetAbsolutePath(string? relativePath) => string.IsNullOrEmpty(relativePath) 
        ? _rootPath 
        : Path.Combine(_rootPath, relativePath);

    /// <inheritdoc />
    public ValueTask<IEnumerable<string>> GetFilesAsync(CancellationToken cancellationToken = default)
    {
        var files = Directory.GetFiles(_rootPath);

        cancellationToken.ThrowIfCancellationRequested();

        return ValueTask.FromResult(files.Select(x => x[(_rootPath.Length + 1)..]));
    }

    /// <inheritdoc />
    public ValueTask<bool> ExistsAsync(string fileName, CancellationToken cancellationToken = default)
    {
        var absPath = GetAbsolutePath(fileName);

        cancellationToken.ThrowIfCancellationRequested();

        return ValueTask.FromResult(File.Exists(absPath));
    }

    /// <inheritdoc />
    public async ValueTask<byte[]> ReadAllBytesAsync(string fileName, CancellationToken cancellationToken = default)
    {
        var absPath = GetAbsolutePath(fileName);

        return await File.ReadAllBytesAsync(absPath, cancellationToken);
    }

    /// <inheritdoc />
    public async ValueTask WriteAllBytesAsync(string fileName, byte[] bytes, CancellationToken cancellationToken = default)
    {
        var absPath = GetAbsolutePath(fileName);

        await File.WriteAllBytesAsync(absPath, bytes, cancellationToken);
    }

    /// <inheritdoc />
    public ValueTask DeleteAsync(string fileName, CancellationToken cancellationToken = default)
    {
        var absPath = GetAbsolutePath(fileName);
        
        cancellationToken.ThrowIfCancellationRequested();

        File.Delete(absPath);

        return ValueTask.CompletedTask;
    }

    /// <inheritdoc />
    public ValueTask RenameAsync(string fileName, string actualFileName, CancellationToken cancellationToken = default)
    {
        var absOldPath = GetAbsolutePath(fileName);
        var absNewPath = GetAbsolutePath(actualFileName);
        
        cancellationToken.ThrowIfCancellationRequested();

        File.Move(absOldPath, absNewPath);
        
        return ValueTask.CompletedTask;
    }
}