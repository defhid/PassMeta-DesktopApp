namespace PassMeta.DesktopApp.Common.Models.Entities.PassFile;

/// <summary>
/// Passfile content information.
/// </summary>
public readonly struct PassFileContent<TData>
    where TData : class
{
    /// <summary>
    /// Decrypted content.
    /// Null if passfile is not decrypted.
    /// </summary>
    public readonly TData? Decrypted;

    /// <summary>
    /// Encrypted content.
    /// Null if passfile is not encrypted or not loaded.
    /// </summary>
    public readonly byte[]? Encrypted;

    /// <summary>
    /// Secret key for encrypting/decrypting content.
    /// Can not be null if <see cref="Decrypted"/> is not null.
    /// </summary>
    public readonly string? PassPhrase;

    /// <summary>
    /// Initialize unloaded content.
    /// </summary>
    public PassFileContent()
        : this(null!, null!, null!)
    {
    }

    /// <summary>
    /// Initialize content with encrypted data.
    /// </summary>
    public PassFileContent(byte[] encrypted)
        : this(null!, encrypted, null!)
    {
    }

    /// <summary>
    /// Initialize content with decrypted data and passphrase to encrypt it.
    /// </summary>
    public PassFileContent(TData decrypted, string passPhrase)
        : this(decrypted, null!, passPhrase)
    {
    }

    /// <summary>
    /// Initialize content with decrypted data, encrypted data and passphrase to encrypt and decrypt them.
    /// </summary>
    public PassFileContent(TData decrypted, byte[] encrypted, string passPhrase)
    {
        Decrypted = decrypted;
        Encrypted = encrypted;
        PassPhrase = passPhrase;
    }
}