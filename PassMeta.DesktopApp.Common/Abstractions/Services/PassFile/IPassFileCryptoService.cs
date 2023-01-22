using PassMeta.DesktopApp.Common.Abstractions.Entities.PassFile;

namespace PassMeta.DesktopApp.Common.Abstractions.Services.PassFile;

/// <summary>
/// Service for passfile encryption and decryption.
/// </summary>
public interface IPassFileCryptoService
{
    /// <summary>
    /// Decrypts <see cref="IPassFile{TContent}.ContentEncrypted"/>,
    /// sets <see cref="IPassFile{TContent}.Content"/>
    /// and returns result.
    /// </summary>
    /// <param name="passFile">Passfile which data to decrypt.</param>
    /// <param name="passPhrase">Phrase to use for decryption.</param>
    /// <param name="silent">Not to write failure logs.</param>
    /// <remarks>
    /// Passfile <see cref="IPassFile{TContent}.PassPhrase"/>
    /// or <paramref name="passPhrase"/> parameter must not be null.
    /// </remarks>
    IDetailedResult Decrypt<TContent>(IPassFile<TContent> passFile, string? passPhrase = null, bool silent = false)
        where TContent : class;

    /// <summary>
    /// Encrypts <see cref="IPassFile{TContent}.Content"/>,
    /// sets <see cref="IPassFile{TContent}.ContentEncrypted"/>
    /// and returns result.
    /// </summary>
    /// <remarks>
    /// Passfile <see cref="IPassFile{TContent}.PassPhrase"/>
    /// or <paramref name="passPhrase"/> parameter must not be null.
    /// </remarks>
    IDetailedResult Encrypt<TContent>(IPassFile<TContent> passFile, string? passPhrase = null)
        where TContent : class;
}