using PassMeta.DesktopApp.Common.Abstractions.Entities.PassFile;

namespace PassMeta.DesktopApp.Common.Models.Entities.PassFile;

/// <inheritdoc />
public abstract class PassFileContent<TData> : IPassFileContent<TData>
    where TData : class
{
    /// <summary>
    /// Initialize content with decrypted data, encrypted data and passphrase to encrypt and decrypt them.
    /// </summary>
    protected PassFileContent(TData? dataDecrypted, byte[]? dataEncrypted, string? passPhrase)
    {
        DataDecrypted = dataDecrypted;
        DataEncrypted = dataEncrypted;
        PassPhrase = passPhrase;
    }

    /// <inheritdoc />
    public TData? DataDecrypted { get; }

    /// <inheritdoc />
    public byte[]? DataEncrypted { get; }

    /// <inheritdoc />
    public string? PassPhrase { get; }
}