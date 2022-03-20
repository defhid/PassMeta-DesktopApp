namespace PassMeta.DesktopApp.Common.Interfaces.Services.PassFile
{
    using System.Threading.Tasks;

    /// <summary>
    /// Service for working with passfiles.
    /// </summary>
    public interface IPassFileService
    {
        /// <summary>
        /// Refresh passfiles from remote and get result list from local storage.
        /// </summary>
        /// <remarks>Automatic errors showing.</remarks>
        /// <returns>Actual current passfile list.</returns>
        Task RefreshLocalPassFilesAsync(bool applyLocalChanges);

        /// <summary>
        /// Apply changes from local manager and commit locally.
        /// </summary>
        /// <remarks>Automatic errors showing.</remarks>
        /// <returns>Actual current passfile list.</returns>
        Task ApplyPassFileLocalChangesAsync();
    }
}