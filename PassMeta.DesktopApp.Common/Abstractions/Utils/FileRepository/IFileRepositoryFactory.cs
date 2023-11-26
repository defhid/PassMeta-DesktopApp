namespace PassMeta.DesktopApp.Common.Abstractions.Utils.FileRepository;

/// <summary>
/// <see cref="IFileRepository"/> provider.
/// </summary>
/// <remarks>Singleton.</remarks>
public interface IFileRepositoryFactory
{
    /// <summary>
    /// Get a repository for passfiles by user context.
    /// </summary>
    IFileRepository ForPassFiles(string? serverId);

    /// <summary>
    /// Get a repository for application system files.
    /// </summary>
    IFileRepository ForSystemFiles();
}