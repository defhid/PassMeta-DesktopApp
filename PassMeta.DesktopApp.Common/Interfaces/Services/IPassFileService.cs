namespace PassMeta.DesktopApp.Common.Interfaces.Services
{
    using DesktopApp.Common.Models.Entities;
    using System.Collections.Generic;
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
        Task<List<PassFile>> GetPassFileListAsync();

        /// <summary>
        /// Apply changes from local manager and commit locally.
        /// </summary>
        /// <returns>Actual current passfile list.</returns>
        Task<List<PassFile>> ApplyPassFileLocalChangesAsync();
    }
}