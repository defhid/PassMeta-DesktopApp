namespace PassMeta.DesktopApp.Common.Interfaces.Services.PassFile
{
    using System.Threading.Tasks;
    using Enums;
    using Models.Dto;
    using Models.Entities;

    /// <summary>
    /// Service for preparing <see cref="PassFileType.Pwd"/> passfiles for merging.
    /// </summary>
    public interface IPwdMergePreparingService
    {
        /// <summary>
        /// Build <see cref="PwdMerge"/> from enumerable passfile sections.
        /// </summary>
        Task<IResult<PwdMerge>> LoadAndPrepareMergeAsync(PassFile localPassFile);
    }
}