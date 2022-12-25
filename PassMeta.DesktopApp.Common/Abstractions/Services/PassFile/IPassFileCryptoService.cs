namespace PassMeta.DesktopApp.Common.Abstractions.Services.PassFile
{
    using Models.Entities;

    /// <summary>
    /// Service for passfile encryption and decryption.
    /// </summary>
    public interface IPassFileCryptoService
    {
        /// <summary>
        /// Decrypts <see cref="PassFile.DataEncrypted"/> and returns result.
        /// </summary>
        /// <param name="passFile">Passfile which data to decrypt.</param>
        /// <param name="passPhrase">Phrase to use for decryption.</param>
        /// <param name="silent">Not to write failure logs.</param>
        /// <remarks>
        /// <see cref="PassFile.PassPhrase"/> must be not null.
        /// </remarks>
        IDetailedResult Decrypt(PassFile passFile, string? passPhrase = null, bool silent = false);

        /// <summary>
        /// Encrypts <see cref="PassFile.PwdData"/> and sets result to <see cref="PassFile.DataEncrypted"/>.
        /// </summary>
        /// <remarks>
        /// <see cref="PassFile.PassPhrase"/> must be not null.
        /// </remarks>
        IDetailedResult Encrypt(PassFile passFile);
    }
}