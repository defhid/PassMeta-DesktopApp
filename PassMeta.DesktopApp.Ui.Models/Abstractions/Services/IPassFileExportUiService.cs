using System.Threading.Tasks;
using Avalonia.Platform.Storage;
using PassMeta.DesktopApp.Common.Models.Entities.PassFile;

namespace PassMeta.DesktopApp.Ui.Models.Abstractions.Services;

/// <summary>
/// Service for exporting passfiles.
/// </summary>
public interface IPassFileExportUiService<in TPassFile>
    where TPassFile : PassFile
{
    /// <summary>
    /// Select destination file path and export passfile data there.
    /// </summary>
    Task SelectAndExportAsync(TPassFile passFile, IStorageProvider storageProvider);
}