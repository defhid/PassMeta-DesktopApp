using PassMeta.DesktopApp.Common.Models.Entities.PassFile;

namespace PassMeta.DesktopApp.Ui.Interfaces.UiServices
{
    using System.Threading.Tasks;
    using Avalonia.Controls;
    using Common.Abstractions;

    /// <summary>
    /// Service for exporting passfiles.
    /// </summary>
    public interface IPassFileExportUiService
    {
        /// <summary>
        /// Select destination file path and export passfile data there.
        /// </summary>
        Task<IResult> SelectAndExportAsync(PassFile passFile, Window currentWindow);
    }
}