using System.Threading.Tasks;
using Avalonia.Controls;
using PassMeta.DesktopApp.Common.Abstractions;
using PassMeta.DesktopApp.Common.Abstractions.PassFileContext;
using PassMeta.DesktopApp.Common.Models.Entities.PassFile;

namespace PassMeta.DesktopApp.Ui.Interfaces.Services;

/// <summary>
/// Service for merging passfiles.
/// </summary>
public interface IPassFileMergeUiService
{
    /// <summary>
    /// Load required data, prepare and merge passfile data sections.
    /// </summary>
    Task<IResult> LoadRemoteAndMergeAsync(PwdPassFile passFile, IPassFileContext<PwdPassFile> context, Window currentWindow);
}