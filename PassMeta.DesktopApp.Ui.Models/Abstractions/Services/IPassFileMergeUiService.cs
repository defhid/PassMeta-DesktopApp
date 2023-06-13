using System.Threading.Tasks;
using Avalonia.Controls;
using PassMeta.DesktopApp.Common.Abstractions;
using PassMeta.DesktopApp.Common.Abstractions.PassFileContext;
using PassMeta.DesktopApp.Common.Models.Entities.PassFile;
using PassMeta.DesktopApp.Ui.Models.Providers;

namespace PassMeta.DesktopApp.Ui.Models.Abstractions.Services;

/// <summary>
/// Service for merging passfiles.
/// </summary>
public interface IPassFileMergeUiService<TPassFile>
    where TPassFile : PassFile
{
    /// <summary>
    /// Load required data, prepare and merge passfile data sections.
    /// </summary>
    Task<IResult> LoadRemoteAndMergeAsync(
        TPassFile passFile,
        IPassFileContext<TPassFile> context,
        HostWindowProvider windowProvider);
}