using System.IO;
using PassMeta.DesktopApp.Common.Abstractions.Services.Logging;
using PassMeta.DesktopApp.Common.Abstractions.Utils.FileRepository;
using PassMeta.DesktopApp.Core.Services.Extensions;

namespace PassMeta.DesktopApp.Core.Utils.FileRepository;

/// <inheritdoc />
public class FileRepositoryFactory : IFileRepositoryFactory
{
    private readonly ILogService _logger;

    /// <summary></summary>
    public FileRepositoryFactory(ILogService logger)
    {
        _logger = logger;
    }
    
    /// <inheritdoc />
    public IFileRepository ForLocalPassFiles(string? serverId) 
        => CreateLocalRepository(Path.Combine(AppConfig.PassFilesDirectory, "s_" + (serverId ?? "_undefined")));

    /// <inheritdoc />
    public IFileRepository ForLocalSystemFiles() 
        => CreateLocalRepository(AppConfig.RootPath);

    private IFileRepository CreateLocalRepository(string rootPath)
    {
        _logger.Debug("Creating local file provider with root directory '{Path}'", rootPath);

        Directory.CreateDirectory(rootPath);

        return new LocalFileRepository(AppConfig.RootPath);
    }
}