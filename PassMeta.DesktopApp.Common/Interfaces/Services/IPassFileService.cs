namespace PassMeta.DesktopApp.Common.Interfaces.Services
{
    using DesktopApp.Common.Models;
    using DesktopApp.Common.Models.Entities;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public interface IPassFileService
    {
        /// <summary>
        /// Get passfile by its id.
        /// </summary>
        Task<PassFile?> GetPassFileRemoteAsync(int passFileId);
        
        /// <summary>
        /// Refresh passfiles from remote and get result list from local storage.
        /// </summary>
        /// <remarks>
        /// Automatic conflicts merge.
        /// Automatic errors showing.
        /// </remarks>
        Task<Result<List<PassFile>>> GetPassFileListAsync();

        /// <summary>
        /// Save passfile info and data, local and remote.
        /// </summary>
        /// <remarks>
        /// Automatic errors showing.
        /// </remarks>
        Task<Result<PassFile>> SavePassFileAsync(PassFile passFile);

        /// <summary>
        /// Archive remote passfile, get actual and update local storage.
        /// </summary>
        /// <remarks>
        /// Automatic errors showing.
        /// </remarks>
        Task<Result<PassFile>> ArchivePassFileAsync(PassFile passFile);
        
        /// <summary>
        /// Unarchive remote passfile, get actual and update local storage.
        /// </summary>
        /// <remarks>
        /// Automatic errors showing.
        /// </remarks>
        Task<Result<PassFile>> UnArchivePassFileAsync(PassFile passFile);

        /// <summary>
        /// Delete local and remote passfile.
        /// </summary>
        /// <remarks>
        /// Automatic errors showing.
        /// </remarks>
        Task<Result> DeletePassFileAsync(PassFileLight passFile, string accountPassword);
    }
}