using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using PassMeta.DesktopApp.Common.Abstractions.Services;

namespace PassMeta.DesktopApp.Core.Services;

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
    private static readonly byte[] CryptoSalt = Encoding.UTF8.GetBytes("PassMetaFileSalt");

    /// <summary>
    /// Make encryption/decryption key for specific iteration by keyphrase.
    /// </summary>
    private static byte[] MakeKey(int iteration, string keyPhrase)
    {
        var offset = (CryptoK + iteration) % keyPhrase.Length;
        var key = keyPhrase[..offset] + Math.Pow(CryptoK - iteration, iteration % 5) + keyPhrase[offset..];
                        
        return SHA256.HashData(Encoding.Unicode.GetBytes(key));
    }
    
    /// <inheritdoc />
    public byte[] Encrypt(byte[] data, string keyPhrase)
    {
        var encryption = data;

        using var aes = Aes.Create();
        aes.IV = CryptoSalt;
                
        for (var i = 0; i < CryptoK; ++i)
        {
            aes.Key = MakeKey(i, keyPhrase);

            using var encryptor = aes.CreateEncryptor();
            using var ms = new MemoryStream();
            using (var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
            {
                cs.Write(encryption, 0, encryption.Length);
                cs.Flush();
            }

            encryption = ms.ToArray();
        }

        return encryption;
    }

    /// <inheritdoc />
    public byte[] Decrypt(byte[] data, string keyPhrase)
    {
        var decryption = data;
            
        using var aes = Aes.Create();
        aes.IV = CryptoSalt;

        for (var i = CryptoK - 1; i >= 0; --i)
        {
            aes.Key = MakeKey(i, keyPhrase);

            using var decryptor = aes.CreateDecryptor();
            using var ms = new MemoryStream(decryption);
            using var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read);
            using var msResult = new MemoryStream();

            cs.CopyTo(msResult);
            decryption = msResult.ToArray();
        }

        return decryption;
    }
}