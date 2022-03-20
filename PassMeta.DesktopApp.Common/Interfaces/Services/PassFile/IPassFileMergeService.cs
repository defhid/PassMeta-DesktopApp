namespace PassMeta.DesktopApp.Common.Interfaces.Services.PassFile
{
    using System.Threading.Tasks;
    using Models.Dto;
    using Models.Entities;

    /// <summary>
    /// Service for preparing passfiles for merging.
    /// </summary>
    public interface IPassFileMergeService
    {
        /// <summary>
        /// Build <see cref="PassFileMerge"/> from enumerable passfile sections.
        /// </summary>
        Task<IResult<PassFileMerge>> LoadRemoteAndPrepareAsync(PassFile localPassFile);
    }
}