namespace PassMeta.DesktopApp.Common.Abstractions.Services.PassMetaServices;

/// <summary>
/// Service that provides PassMeta crypto-methods.
/// </summary>
public interface IPassMetaCryptoService
{
    /// <summary>
    /// Encrypt data from decrypted bytes with key phrase.
    /// </summary>
    byte[] Encrypt(byte[] data, string keyPhrase);

    /// <summary>
    /// Decrypt data from encrypted bytes with key phrase.
    /// </summary>
    byte[] Decrypt(byte[] data, string keyPhrase);
}