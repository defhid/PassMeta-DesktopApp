using System.Threading.Tasks;
using PassMeta.DesktopApp.Common.Abstractions.PassFileContext;
using PassMeta.DesktopApp.Common.Models.Entities.PassFile;

namespace PassMeta.DesktopApp.Common.Abstractions.Utils.Helpers;

/// <summary>
/// A helper for decrypting passfiles.
/// </summary>
public interface IPassFileDecryptionHelper
{
    /// <summary>
    /// If <paramref name="passFile"/> has decrypted content: return it.
    /// Else, provide encrypted content and decrypt it (perhaps with a passphrase asking).
    /// </summary>
    ValueTask<IResult> ProvideDecryptedContentAsync<TPassFile>(
        TPassFile passFile,
        IPassFileContext<TPassFile> context,
        (string? Question, string? RepeatQuestion) questions = default)
        where TPassFile : PassFile;
}