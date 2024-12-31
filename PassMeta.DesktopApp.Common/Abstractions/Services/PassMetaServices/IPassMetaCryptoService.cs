using System.Threading;
using System.Threading.Tasks;

namespace PassMeta.DesktopApp.Common.Abstractions.Services.PassMetaServices;

/// <summary>
/// Service that provides PassMeta crypto-methods.
/// </summary>
public interface IPassMetaCryptoService
{
    /// <summary>
    /// Encrypt data from decrypted bytes with key phrase.
    /// </summary>
    Task<byte[]> EncryptAsync(byte[] data, string keyPhrase, CancellationToken ct = default);

    /// <summary>
    /// Decrypt data from encrypted bytes with key phrase.
    /// </summary>
    Task<byte[]> DecryptAsync(byte[] data, string keyPhrase, CancellationToken ct = default);
}