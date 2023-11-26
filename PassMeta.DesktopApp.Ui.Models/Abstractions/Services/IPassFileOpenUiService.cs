using System.Threading.Tasks;
using PassMeta.DesktopApp.Common.Models.Entities.PassFile;
using PassMeta.DesktopApp.Ui.Models.Abstractions.Providers;

namespace PassMeta.DesktopApp.Ui.Models.Abstractions.Services;

/// <summary>
/// Service for opening passfiles.
/// </summary>
public interface IPassFileOpenUiService<in TPassFile>
    where TPassFile : PassFile
{
    /// <summary>
    /// Show passfile information.
    /// </summary>
    Task ShowInfoAsync(TPassFile passFile, IHostWindowProvider windowProvider);
}