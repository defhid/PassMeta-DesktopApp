namespace PassMeta.DesktopApp.Common.Abstractions.Entities.PassFile;

/// <summary>
/// Passfile content information.
/// </summary>
public interface IPassFileContent<out TData>
    where TData : class
{
    /// <summary>
    /// Decrypted content.
    /// Null if passfile is not decrypted.
    /// </summary>
    TData? DataDecrypted { get; }

    /// <summary>
    /// Encrypted content.
    /// Null if passfile is not encrypted or not loaded.
    /// </summary>
    byte[]? DataEncrypted { get; }

    /// <summary>
    /// Secret key for encrypting/decrypting content.
    /// Can not be null if <see cref="DataDecrypted"/> is not null.
    /// </summary>
    string? PassPhrase { get; }

    /// <summary>
    /// Make a deep copy of <see cref="DataDecrypted"/>
    /// </summary>
    TData? CloneDecryptedData();
}