namespace PassMeta.DesktopApp.Common.Interfaces.Services.PassFile
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Enums;
    using Models;
    using Models.Entities;

    /// <summary>
    /// Service for working with remote passfiles.
    /// </summary>
    public interface IPassFileRemoteService
    {
        /// <summary>
        /// Load passfile with encrypted data from the server.
        /// </summary>
        Task<IResult<PassFile>> GetAsync(int passFileId);

        /// <summary>
        /// Load passfile encrypted data.
        /// </summary>
        Task<OkBadResponse<string>?> GetDataAsync(int passFileId, int version);

        /// <summary>
        /// Load user's passfile list of specified type without encrypted data.
        /// </summary>
        Task<List<PassFile>?> GetListAsync(PassFileType ofType);

        /// <summary>
        /// Save passfile information fields.
        /// </summary>
        Task<OkBadResponse<PassFile>?> SaveInfoAsync(PassFile passFile);

        /// <summary>
        /// Save passfile data fields with encrypted data.
        /// </summary>
        Task<OkBadResponse<PassFile>?> SaveDataAsync(PassFile passFile);

        /// <summary>
        /// Add a new passfile.
        /// </summary>
        Task<OkBadResponse<PassFile>?> AddAsync(PassFile passFile);

        /// <summary>
        /// Delete passfile.
        /// </summary>
        Task<OkBadResponse?> DeleteAsync(PassFile passFile, string accountPassword);
    }
}