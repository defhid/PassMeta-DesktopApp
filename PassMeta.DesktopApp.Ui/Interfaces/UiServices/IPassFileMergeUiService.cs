namespace PassMeta.DesktopApp.Ui.Interfaces.UiServices
{
    using System.Threading.Tasks;
    using Avalonia.Controls;
    using PassMeta.DesktopApp.Common.Interfaces;
    using PassMeta.DesktopApp.Common.Models.Entities;

    /// <summary>
    /// Service for merging passfiles.
    /// </summary>
    public interface IPassFileMergeUiService
    {
        /// <summary>
        /// Load required data, prepare and merge passfile data sections.
        /// </summary>
        Task<IResult> LoadRemoteAndMergeAsync(PassFile passFile, Window currentWindow);
    }
}