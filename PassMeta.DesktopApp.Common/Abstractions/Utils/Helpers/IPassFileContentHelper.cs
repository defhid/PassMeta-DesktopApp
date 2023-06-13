using System.Threading.Tasks;
using PassMeta.DesktopApp.Common.Models.Entities.PassFile;

namespace PassMeta.DesktopApp.Common.Abstractions.Utils.Helpers;

/// <summary>
/// Service for working with passfile content.
/// </summary>
/// <remarks>Singleton.</remarks>
public interface IPassFileContentHelper<in TPassFile>
    where TPassFile : PassFile
{
    /// <summary>
    /// Ask user a new passphrase for <paramref name="passFile"/> and
    /// apply to its current content.
    /// </summary>
    Task<IResult> ChangePassPhraseAsync(TPassFile passFile);
}