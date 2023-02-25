using System.Threading.Tasks;
using Avalonia.Controls;
using PassMeta.DesktopApp.Common.Models.Entities.PassFile;

namespace PassMeta.DesktopApp.Ui.Interfaces.Services;

/// <summary>
/// Service for exporting passfiles.
/// </summary>
public interface IPassFileExportUiService
{
    /// <summary>
    /// Select destination file path and export passfile data there.
    /// </summary>
    Task SelectAndExportAsync<TContent>(PassFile<TContent> passFile, Window currentWindow)
        where TContent : class, new();
}