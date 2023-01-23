using System.Threading.Tasks;
using PassMeta.DesktopApp.Common.Enums;

namespace PassMeta.DesktopApp.Common.Abstractions.Services.PassFileServices;

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