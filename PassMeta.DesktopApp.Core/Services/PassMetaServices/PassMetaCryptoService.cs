using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using PassMeta.DesktopApp.Common.Abstractions.Services.PassMetaServices;

namespace PassMeta.DesktopApp.Core.Services.PassMetaServices;

/// <inheritdoc />
public class PassMetaCryptoService : IPassMetaCryptoService
{
    /// <summary>
    /// Number of encryption iterations.
    /// </summary>
    private const int CryptoK = 100;
            
    /// <summary>
    /// Encryption salt.
    /// </summary>
    private static readonly byte[] CryptoSalt = "PassMetaFileSalt"u8.ToArray();

    /// <summary>
    /// Make encryption/decryption key for specific iteration by keyphrase.
    /// </summary>
    private static byte[] MakeKey(int iteration, string keyPhrase)
    {
        var offset = (CryptoK + iteration) % keyPhrase.Length;
        var key = keyPhrase[..offset] + Math.Pow(CryptoK - iteration, iteration % 5) + keyPhrase[offset..];
                        
        return SHA256.HashData(Encoding.UTF8.GetBytes(key));
    }
    
    /// <inheritdoc />
    public async Task<byte[]> EncryptAsync(byte[] data, string keyPhrase, CancellationToken ct)
    {
        var encryption = data;

        using var aes = Aes.Create();
        aes.IV = CryptoSalt;
                
        for (var i = 0; i < CryptoK; ++i)
        {
            aes.Key = MakeKey(i, keyPhrase);

            using var encryptor = aes.CreateEncryptor();
            using var ms = new MemoryStream();
            await using (var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
            {
                await cs.WriteAsync(encryption, ct);
                await cs.FlushAsync(ct);
            }

            encryption = ms.ToArray();
        }

        return encryption;
    }

    /// <inheritdoc />
    public async Task<byte[]> DecryptAsync(byte[] data, string keyPhrase, CancellationToken ct)
    {
        var decryption = data;
            
        using var aes = Aes.Create();
        aes.IV = CryptoSalt;

        for (var i = CryptoK - 1; i >= 0; --i)
        {
            aes.Key = MakeKey(i, keyPhrase);

            using var decryptor = aes.CreateDecryptor();
            using var ms = new MemoryStream(decryption);
            await using var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read);
            using var msResult = new MemoryStream();

            await cs.CopyToAsync(msResult, ct);
            decryption = msResult.ToArray();
        }

        return decryption;
    }
}