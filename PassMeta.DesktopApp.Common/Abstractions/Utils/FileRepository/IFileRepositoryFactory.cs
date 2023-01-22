namespace PassMeta.DesktopApp.Common.Abstractions.Utils.FileRepository;

/// <summary>
/// <see cref="IFileRepository"/> provider.
/// </summary>
/// <remarks>Singleton.</remarks>
public interface IFileRepositoryFactory
{
    /// <summary>
    /// Get a repository for local passfiles by user context.
    /// </summary>
    IFileRepository ForLocalPassFiles(string? serverId);
    
    /// <summary>
    /// Get a repository for local system files.
    /// </summary>
    IFileRepository ForLocalSystemFiles();
}