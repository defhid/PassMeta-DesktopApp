using System.Collections.Generic;
using System.Threading.Tasks;
using PassMeta.DesktopApp.Common.Models;
using PassMeta.DesktopApp.Common.Models.Entities;

namespace PassMeta.DesktopApp.Common.Interfaces.Services
{
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
        /// Save passfile local and remote.
        /// </summary>
        /// <param name="passFile"></param>
        /// <remarks>
        /// Automatic errors showing.
        /// </remarks>
        Task<Result> SavePassFileAsync(PassFile passFile);
        
        /// <summary>
        /// Archive remote passfile, get actual and update local storage.
        /// </summary>
        /// <remarks>
        /// Automatic errors showing.
        /// </remarks>
        Task<Result<PassFile?>> ArchivePassFileAsync(PassFileLight passFile);
        
        /// <summary>
        /// Unarchive remote passfile, get actual and update local storage.
        /// </summary>
        /// <remarks>
        /// Automatic errors showing.
        /// </remarks>
        Task<Result<PassFile?>> UnArchivePassFileAsync(PassFileLight passFile);

        /// <summary>
        /// Delete local and remote passfile.
        /// </summary>
        /// <remarks>
        /// Automatic errors showing.
        /// </remarks>
        Task<Result> DeletePassFileAsync(PassFileLight passFile, string accountPassword);
    }
}