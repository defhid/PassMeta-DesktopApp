namespace PassMeta.DesktopApp.Common.Abstractions.Services.PassFile
{
    using System.Threading.Tasks;
    using PassMeta.DesktopApp.Common.Enums;
    using PassMeta.DesktopApp.Common.Models.Dto;
    using PassMeta.DesktopApp.Common.Models.Entities;

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