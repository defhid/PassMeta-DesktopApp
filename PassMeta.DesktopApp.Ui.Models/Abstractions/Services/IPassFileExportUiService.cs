using System.Threading.Tasks;
using PassMeta.DesktopApp.Common.Models.Entities.PassFile;
using PassMeta.DesktopApp.Ui.Models.Abstractions.Providers;

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
    Task SelectAndExportAsync(TPassFile passFile, IHostWindowProvider windowProvider);
}