using PassMeta.DesktopApp.Common.Models.Entities.PassFile;

namespace PassMeta.DesktopApp.Ui.Interfaces.UiServices
{
    using System.Threading.Tasks;
    using Avalonia.Controls;
    using Common.Abstractions;

    /// <summary>
    /// Service for restoring / importing passfiles.
    /// </summary>
    public interface IPassFileRestoreUiService
    {
        /// <summary>
        /// Select required file and import its data.
        /// </summary>
        Task<IResult> SelectAndRestoreAsync(PassFile passFile, Window currentWindow);
    }
}