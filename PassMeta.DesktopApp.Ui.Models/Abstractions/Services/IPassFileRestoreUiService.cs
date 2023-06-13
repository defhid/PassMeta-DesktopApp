using System.Threading.Tasks;
using PassMeta.DesktopApp.Common.Abstractions;
using PassMeta.DesktopApp.Common.Abstractions.PassFileContext;
using PassMeta.DesktopApp.Common.Models.Entities.PassFile;
using PassMeta.DesktopApp.Ui.Models.Providers;

namespace PassMeta.DesktopApp.Ui.Models.Abstractions.Services;

/// <summary>
/// Service for restoring / importing passfiles.
/// </summary>
public interface IPassFileRestoreUiService<TPassFile>
    where TPassFile : PassFile
{
    /// <summary>
    /// Select required file and import its data.
    /// </summary>
    Task<IResult> SelectAndRestoreAsync(
        TPassFile passFile,
        IPassFileContext<TPassFile> pfContext,
        HostWindowProvider windowProvider);
}