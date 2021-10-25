using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using PassMeta.DesktopApp.Common.Interfaces.Services;
using PassMeta.DesktopApp.Core.Utils;
using Aes = System.Security.Cryptography.Aes;

namespace PassMeta.DesktopApp.Core.Services
{
    public class CryptoService : ICryptoService
    {
        private static readonly Random Random = new();
        
        public string Encrypt(string data, string keyPhrase)
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

        public string Decrypt(string data, string keyPhrase)
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

        public string MakeCheckKey(string keyPhrase)
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

        public string GeneratePassword(int length, bool includeDigits, bool includeSpecial)
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
    }
}