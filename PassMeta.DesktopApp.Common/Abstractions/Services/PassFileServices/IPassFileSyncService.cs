using System.Threading.Tasks;
using PassMeta.DesktopApp.Common.Abstractions.PassFileContext;
using PassMeta.DesktopApp.Common.Models.Entities.PassFile;

namespace PassMeta.DesktopApp.Common.Abstractions.Services.PassFileServices;

/// <summary>
/// Service for synchronizing local and remote passfiles.
/// </summary>
public interface IPassFileSyncService
{
    /// <summary>
    /// Commit current local changes and synchronize them with the remote.
    /// </summary>
    /// <remarks>Results will always be showed by dialog service.</remarks>
    Task SynchronizeAsync<TPassFile>(IPassFileContext<TPassFile> context)
        where TPassFile : PassFile;
}