using System.Threading.Tasks;
using PassMeta.DesktopApp.Common.Enums;
using PassMeta.DesktopApp.Common.Models.Entities.PassFile;
using PassMeta.DesktopApp.Common.Models.Entities.PassFileMerge;

namespace PassMeta.DesktopApp.Common.Abstractions.Services.PassFileServices;

/// <summary>
/// Service for preparing <see cref="PassFileType.Pwd"/> passfiles for merging.
/// </summary>
/// <remarks>Transient.</remarks>
public interface IPwdPassFileMergePreparingService
{
    /// <summary>
    /// Build <see cref="PwdPassFileMerge"/> for local and remote passfile content.
    /// </summary>
    Task<IResult<PwdPassFileMerge>> LoadAndPrepareMergeAsync(PwdPassFile localPassFile);
}