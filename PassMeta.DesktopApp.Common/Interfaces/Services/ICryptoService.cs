namespace PassMeta.DesktopApp.Common.Interfaces.Services
{
    public interface ICryptoService
    {
        /// <summary>
        /// Encrypt data with keyphrase.
        /// </summary>
        /// <returns>Encrypted string or null if error occured.</returns>
        string? Encrypt(string data, string keyPhrase);
        
        /// <summary>
        /// Decrypt data with keyphrase.
        /// </summary>
        /// <returns>Decrypted string or null if error occured.</returns>
        string? Decrypt(string data, string keyPhrase);

        /// <summary>
        /// Make special PassMeta hash string by keyphrase.
        /// </summary>
        /// <returns>Result string or null if error occured.</returns>
        string? MakeCheckKey(string keyPhrase);

        /// <summary>
        /// Generate random password by length, using digits and special symbols.
        /// </summary>
        /// <returns>Generated string or null if error occured.</returns>
        string? GeneratePassword(int length, bool includeDigits, bool includeSpecial);
    }
}