using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using PassMeta.DesktopApp.Common.Interfaces.Services;
using PassMeta.DesktopApp.Core.Utils;

namespace PassMeta.DesktopApp.Core.Services
{
    public class CryptoService : ICryptoService
    {
        private static Random _random = new();
        
        public string Encrypt(string data, string keyPhrase)
        {
            var aes = Aes.Create();
            aes.IV = AppConfig.PassFileSalt;
            aes.Key = Encoding.UTF8.GetBytes(SHA256.Create(keyPhrase)!.ToString()!);
            
            var crypt = aes.CreateEncryptor(aes.Key, aes.IV);
            
            using var ms = new MemoryStream();
            using var cs = new CryptoStream(ms, crypt, CryptoStreamMode.Write);
            
            cs.Write(Encoding.UTF8.GetBytes(data));
            
            return Encoding.UTF8.GetString(ms.ToArray());
        }

        public string Decrypt(string data, string keyPhrase)
        {
            var aes = Aes.Create();
            aes.IV = AppConfig.PassFileSalt;
            aes.Key = Encoding.UTF8.GetBytes(keyPhrase);
            
            var crypt = aes.CreateDecryptor(aes.Key, aes.IV);

            using var ms = new MemoryStream(Encoding.UTF8.GetBytes(data));
            using var cs = new CryptoStream(ms, crypt, CryptoStreamMode.Read);
            using var sr = new StreamReader(cs);
            
            return sr.ReadToEnd();
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
                .Select(s => s[_random.Next(s.Length)]).ToArray());
        }
    }
}