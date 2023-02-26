using System.Threading.Tasks;
using Avalonia.Controls;
using PassMeta.DesktopApp.Common.Abstractions;
using PassMeta.DesktopApp.Common.Abstractions.PassFileContext;
using PassMeta.DesktopApp.Common.Models.Entities.PassFile;

namespace PassMeta.DesktopApp.Ui.Interfaces.Services;

/// <summary>
/// Service for restoring / importing passfiles.
/// </summary>
public interface IPassFileRestoreUiService
{
    /// <summary>
    /// Select required file and import its data.
    /// </summary>
    Task<IResult> SelectAndRestoreAsync(PwdPassFile passFile, IPassFileContext<PwdPassFile> pfContext, Window currentWindow);
}