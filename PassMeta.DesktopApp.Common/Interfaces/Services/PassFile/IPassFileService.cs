namespace PassMeta.DesktopApp.Common.Interfaces.Services.PassFile
{
    using System.Threading.Tasks;
    using Models.Entities;

    /// <summary>
    /// Service for working with passfiles.
    /// </summary>
    public interface IPassFileService
    {
        /// <summary>
        /// Load passfile with encrypted data from the server.
        /// </summary>
        Task<IResult<PassFile>> GetPassFileRemoteAsync(int passFileId);
        
        /// <summary>
        /// Refresh passfiles from remote and get result list from local storage.
        /// </summary>
        /// <remarks>Automatic errors showing.</remarks>
        /// <returns>Actual current passfile list.</returns>
        Task RefreshLocalPassFilesAsync();

        /// <summary>
        /// Apply changes from local manager and commit locally.
        /// </summary>
        /// <remarks>Automatic errors showing.</remarks>
        /// <returns>Actual current passfile list.</returns>
        Task ApplyPassFileLocalChangesAsync();
    }
}