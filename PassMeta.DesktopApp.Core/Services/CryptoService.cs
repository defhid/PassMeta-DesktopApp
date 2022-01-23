namespace PassMeta.DesktopApp.Core.Services
{
    using DesktopApp.Common.Interfaces.Services;
    using DesktopApp.Core.Utils;
    using System;
    using System.IO;
    using System.Linq;
    using System.Security.Cryptography;
    using System.Text;
    using Splat;

    /// <inheritdoc />
    public class CryptoService : ICryptoService
    {
        private static readonly Random Random = new();

        private const int CryptoK = 100;
        
        private readonly ILogService _logger = Locator.Current.GetService<ILogService>()!;

        /// <inheritdoc />
        public string? Encrypt(string data, string keyPhrase)
        {
            try
            {
                var encryption = AppConfig.PassFileEncoding.GetBytes(data);
                
                using (var aes = Aes.Create())
                {
                    aes.IV = AppConfig.PassFileSalt;
                
                    for (var i = 0; i < CryptoK; ++i)
                    {
                        var offset = (CryptoK + i) % keyPhrase.Length;
                        var key = keyPhrase[..offset] + Math.Pow(CryptoK - i, i % 5) + keyPhrase[offset..];
                        
                        aes.Key = SHA256.HashData(AppConfig.PassFileEncoding.GetBytes(key));

                        using var encryptor = aes.CreateEncryptor();
                        using var ms = new MemoryStream();
                        using (var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
                        {
                            cs.Write(encryption);
                            cs.Flush();
                        }

                        encryption = ms.ToArray();
                    }
                }

                return Convert.ToBase64String(encryption);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Encryption failed");
                return null;
            }
        }

        /// <inheritdoc />
        public string? Decrypt(string data, string keyPhrase)
        {
            try
            {
                var decryption = Convert.FromBase64String(data);
                
                using (var aes = Aes.Create())
                {
                    aes.IV = AppConfig.PassFileSalt;
                    
                    for (var i = CryptoK - 1; i >= 0; --i)
                    {
                        var offset = (CryptoK + i) % keyPhrase.Length;
                        var key = keyPhrase[..offset] + Math.Pow(CryptoK - i, i % 5) + keyPhrase[offset..];
                        
                        aes.Key = SHA256.HashData(AppConfig.PassFileEncoding.GetBytes(key));

                        using var decryptor = aes.CreateDecryptor();
                        using var ms = new MemoryStream(decryption);
                        using var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read);
                        using var msResult = new MemoryStream();
                        
                        cs.CopyTo(msResult);
                        decryption = msResult.ToArray();
                    }
                }

                return AppConfig.PassFileEncoding.GetString(decryption);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Decryption failed");
                return null;
            }
        }

        /// <inheritdoc />
        public string? GeneratePassword(int length, bool includeDigits, bool includeSpecial)
        {
            try
            {
                var chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
                chars += chars.ToLower();

                if (includeDigits)
                    chars += "01234567890123456789";

                if (includeSpecial)
                    chars += "*-_!@*-_!@";

                return new string(Enumerable.Repeat(chars, length)
                    .Select(s => s[Random.Next(s.Length)]).ToArray());
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Password generation failed");
                return null;
            }
        }
    }
}