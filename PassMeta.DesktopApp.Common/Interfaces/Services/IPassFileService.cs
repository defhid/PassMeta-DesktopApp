namespace PassMeta.DesktopApp.Common.Interfaces.Services
{
    using DesktopApp.Common.Models;
    using DesktopApp.Common.Models.Entities;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    /// <summary>
    /// Service for working with passfiles.
    /// </summary>
    public interface IPassFileService
    {
        /// <summary>
        /// Get passfile with encrypted data by its id from server.
        /// </summary>
        /// <remarks>Automatic errors showing.</remarks>
        Task<PassFile?> GetPassFileWithDataRemoteAsync(int passFileId);

        /// <summary>
        /// Refresh passfiles from remote and get result list from local storage.
        /// </summary>
        /// <remarks>
        /// Automatic conflicts merge.
        /// Automatic errors showing.
        /// </remarks>
        Task<List<PassFile>> GetPassFileListAsync();

        /// <summary>
        /// Save passfile info, local and remote.
        /// </summary>
        /// <remarks>Automatic errors showing.</remarks>
        Task<PassFile> SavePassFileInfoAsync(PassFile passFile);

        /// <summary>
        /// Save passfile data, local and remote.
        /// </summary>
        /// <remarks>Automatic errors showing.</remarks>
        Task<PassFile> SavePassFileDataAsync(PassFile passFile);
        
        /// <summary>
        /// Delete local and remote passfile.
        /// </summary>
        /// <remarks>Automatic errors showing.</remarks>
        Task<Result> DeletePassFileAsync(PassFile passFile, string? accountPassword);
    }
}