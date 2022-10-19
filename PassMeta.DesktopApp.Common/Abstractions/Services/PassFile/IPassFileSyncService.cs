namespace PassMeta.DesktopApp.Common.Abstractions.Services.PassFile
{
    using System.Threading.Tasks;
    using PassMeta.DesktopApp.Common.Enums;

    /// <summary>
    /// Service for working with passfiles.
    /// </summary>
    public interface IPassFileSyncService
    {
        /// <summary>
        /// Refresh passfiles from remote and get result list from local storage.
        /// </summary>
        /// <remarks>Automatic errors showing.</remarks>
        /// <returns>Actual current passfile list.</returns>
        Task RefreshLocalPassFilesAsync(PassFileType passFileType);

        /// <summary>
        /// Apply changes from local manager and commit locally.
        /// </summary>
        /// <remarks>Automatic errors showing.</remarks>
        /// <returns>Actual current passfile list.</returns>
        Task ApplyPassFileLocalChangesAsync(PassFileType passFileType);
    }
}