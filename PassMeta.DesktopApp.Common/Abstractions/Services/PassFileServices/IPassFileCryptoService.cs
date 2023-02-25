using PassMeta.DesktopApp.Common.Models.Entities.PassFile;

namespace PassMeta.DesktopApp.Common.Abstractions.Services.PassFileServices;

/// <summary>
/// Service for passfile encryption and decryption.
/// </summary>
/// <remarks>Singleton.</remarks>
public interface IPassFileCryptoService
{
    /// <summary>
    /// Decrypts <see cref="PassFileContent{TData}.Encrypted"/>,
    /// sets <see cref="PassFileContent{TData}.Decrypted"/>.
    /// </summary>
    /// <param name="passFile">Passfile which data to decrypt.</param>
    /// <param name="passPhrase">Phrase to use for decryption.</param>
    /// <param name="silent">Not to write failure logs.</param>
    /// <remarks>
    /// Passfile <see cref="PassFileContent{TData}.PassPhrase"/>
    /// or <paramref name="passPhrase"/> parameter must not be null.
    /// </remarks>
    IDetailedResult Decrypt<TContent>(PassFile<TContent> passFile, string? passPhrase = null, bool silent = false)
        where TContent : class, new();

    /// <summary>
    /// Encrypts <see cref="PassFileContent{TData}.Decrypted"/>,
    /// sets <see cref="PassFileContent{TData}.Encrypted"/>.
    /// </summary>
    /// <remarks>
    /// Passfile <see cref="PassFileContent{TData}.PassPhrase"/>
    /// or <paramref name="passPhrase"/> parameter must not be null.
    /// </remarks>
    IDetailedResult Encrypt<TContent>(PassFile<TContent> passFile, string? passPhrase = null)
        where TContent : class, new();
}