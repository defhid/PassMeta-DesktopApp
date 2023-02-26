using System.IO;
using PassMeta.DesktopApp.Common.Abstractions.Utils.FileRepository;
using PassMeta.DesktopApp.Common.Abstractions.Utils.Logging;
using PassMeta.DesktopApp.Common.Extensions;

namespace PassMeta.DesktopApp.Core.Utils.FileRepository;

/// <inheritdoc />
public class FileRepositoryFactory : IFileRepositoryFactory
{
    private readonly ILogsWriter _logger;

    /// <summary></summary>
    public FileRepositoryFactory(ILogsWriter logger)
    {
        _logger = logger;
    }

    /// <inheritdoc />
    public IFileRepository ForPassFiles(string? serverId)
        => CreateLocalRepository(Path.Combine(AppInfo.RootPath, ".passfiles", "s_" + (serverId ?? "_undefined")));

    /// <inheritdoc />
    public IFileRepository ForSystemFiles()
        => CreateLocalRepository(Path.Combine(AppInfo.RootPath, ".app"));

    private IFileRepository CreateLocalRepository(string rootPath)
    {
        _logger.Debug("Creating local file provider with root directory '{Path}'", rootPath);

        Directory.CreateDirectory(rootPath);

        return new LocalFileRepository(rootPath);
    }
}