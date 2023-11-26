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
    /// Has <see cref="Decrypted"/> or <see cref="Encrypted"/> content.
    /// </summary>
    public bool Any => PassPhrase is not null;

    private PassFileContent(TData? decrypted, byte[]? encrypted, string? passPhrase)
    {
        Decrypted = decrypted;
        Encrypted = encrypted;
        PassPhrase = passPhrase;
    }

    /// <summary>
    /// Initialize unloaded content.
    /// </summary>
    public PassFileContent()
        : this(null, null, null)
    {
    }

    /// <summary>
    /// Initialize content with encrypted data.
    /// </summary>
    public PassFileContent(byte[] encrypted)
        : this(null, encrypted, null)
    {
    }

    /// <summary>
    /// Initialize content with encrypted data and passphrase to decrypt it.
    /// </summary>
    public PassFileContent(byte[] encrypted, string passPhrase)
        : this(null, encrypted, passPhrase)
    {
    }

    /// <summary>
    /// Initialize content with decrypted data and passphrase to encrypt it.
    /// </summary>
    public PassFileContent(TData decrypted, string passPhrase)
        : this(decrypted, null, passPhrase)
    {
    }
}