namespace PassMeta.DesktopApp.Core.Services
{
    using DesktopApp.Common.Interfaces.Services;
    using DesktopApp.Core.Utils;
    using Aes = System.Security.Cryptography.Aes;
    using System;
    using System.IO;
    using System.Linq;
    using System.Security.Cryptography;
    using System.Text;
    using Splat;

    public class CryptoService : ICryptoService
    {
        private static readonly Random Random = new();
        
        /// <inheritdoc />
        public string? Encrypt(string data, string keyPhrase)
        {
            try
            {
                byte[] encrypted;

                using (var aes = Aes.Create())
                {
                    aes.IV = AppConfig.PassFileSalt;
                    aes.Key = SHA256.HashData(Encoding.UTF8.GetBytes(keyPhrase));

                    var encryptor = aes.CreateEncryptor(aes.Key, aes.IV);

                    using (var ms = new MemoryStream())
                    using (var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
                    {
                        using (var swEncrypt = new StreamWriter(cs, Encoding.UTF8))
                        {
                            swEncrypt.Write(data);
                        }

                        encrypted = ms.ToArray();
                    }
                }

                return Convert.ToBase64String(encrypted);
            }
            catch (Exception ex)
            {
                Locator.Current.GetService<ILogService>()!.Error(ex, "Encryption failed");
                return null;
            }
        }

        /// <inheritdoc />
        public string? Decrypt(string data, string keyPhrase)
        {
            try
            {
                using var aes = Aes.Create();
            
                aes.IV = AppConfig.PassFileSalt;
                aes.Key = SHA256.HashData(Encoding.UTF8.GetBytes(keyPhrase));

                var decryptor = aes.CreateDecryptor(aes.Key, aes.IV);

                using var ms = new MemoryStream(Convert.FromBase64String(data));
                using var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read);
                using var sr = new StreamReader(cs, Encoding.UTF8);
            
                return sr.ReadToEnd();
            }
            catch (Exception ex)
            {
                Locator.Current.GetService<ILogService>()!.Error(ex, "Decryption failed");
                return null;
            }
        }

        /// <inheritdoc />
        public string? MakeCheckKey(string keyPhrase)
        {
            try
            {
                var keyPhraseBytes = Encoding.UTF8.GetBytes(keyPhrase);
                var bytes = SHA512.HashData(keyPhraseBytes).Concat(SHA256.HashData(keyPhraseBytes)).ToArray();
                for (var i = 0; i < 88; ++i)
                {
                    if (bytes[i] == 0x00)
                    {
                        bytes[i] = (byte)(i % 254 + 1);
                    }
                }

                return Convert.ToBase64String(bytes);
            }
            catch (Exception ex)
            {
                Locator.Current.GetService<ILogService>()!.Error(ex, "Check key making failed");
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
                Locator.Current.GetService<ILogService>()!.Error(ex, "Password generation failed");
                return null;
            }
        }
    }
}