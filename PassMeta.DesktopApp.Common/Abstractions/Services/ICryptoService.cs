namespace PassMeta.DesktopApp.Common.Abstractions.Services
{
    /// <summary>
    /// Service that provides PassMeta crypto-methods.
    /// </summary>
    public interface ICryptoService
    {
        /// <summary>
        /// Encrypt data from decrypted string with keyphrase.
        /// </summary>
        /// <returns>Encrypted string or null if error occured.</returns>
        string? Encrypt(string data, string keyPhrase);
        
        /// <summary>
        /// Decrypt data from encrypted string with keyphrase.
        /// </summary>
        /// <returns>Decrypted string or null if error occured.</returns>
        string? Decrypt(string data, string keyPhrase, bool silent = false);
        
        /// <summary>
        /// Decrypt data from bytes with keyphrase.
        /// </summary>
        /// <returns>Decrypted string or null if error occured.</returns>
        string? Decrypt(byte[] data, string keyPhrase, bool silent = false);

        /// <summary>
        /// Generate random password by length, using digits and special symbols.
        /// </summary>
        /// <returns>Generated string.</returns>
        string GeneratePassword(int length, bool digits, bool lowercase, bool uppercase, bool special);
    }
}