using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using Newtonsoft.Json;
using PassMeta.DesktopApp.Common;
using PassMeta.DesktopApp.Common.Models.Entities;
using PassMeta.DesktopApp.Core.Utils;

namespace PassMeta.DesktopApp.Core.Extensions
{
    public static class PassFileCryptoExtensions
    {
        /// <summary>
        /// Decrypt <see cref="PassFile.Data"/> and return result.
        /// </summary>
        /// <remarks>
        /// <see cref="PassFile.KeyPhrase"/> must be not null.
        /// </remarks>
        public static PassFileData Decrypt(this PassFile passFile)
        {
            if (string.IsNullOrEmpty(passFile.KeyPhrase))
                throw new NullReferenceException("Using Decrypt method without key phrase!");
            
            var aes = Aes.Create();
            aes.IV = AppConfig.PassFileSalt;
            aes.Key = Encoding.UTF8.GetBytes(passFile.KeyPhrase);
            
            string content;
            var crypt = aes.CreateDecryptor(aes.Key, aes.IV);
            
            using (var ms = new MemoryStream(Encoding.UTF8.GetBytes(passFile.Data)))
            {
                using var cs = new CryptoStream(ms, crypt, CryptoStreamMode.Read);
                using var sr = new StreamReader(cs);
                content = sr.ReadToEnd();
            }

            try
            {
                return JsonConvert.DeserializeObject<PassFileData>(content);
            }
            catch
            {
                throw new FormatException(Resources.ERR__PASSFILE_DECRYPTION);
            }
        }
        
        /// <summary>
        /// Encrypt <paramref name="data"/> and set result to <see cref="PassFile.Data"/>.
        /// </summary>
        /// <remarks>
        /// <see cref="PassFile.KeyPhrase"/> must be not null.
        /// </remarks>
        public static void Encrypt(this PassFile passFile, PassFileData data)
        {
            if (string.IsNullOrEmpty(passFile.KeyPhrase))
                throw new NullReferenceException("Using Encrypt method without key phrase!");
            
            var aes = Aes.Create();
            aes.IV = AppConfig.PassFileSalt;
            aes.Key = Encoding.UTF8.GetBytes(passFile.KeyPhrase);
            
            var crypt = aes.CreateEncryptor(aes.Key, aes.IV);
            
            using var ms = new MemoryStream();
            using var cs = new CryptoStream(ms, crypt, CryptoStreamMode.Write);
            
            cs.Write(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(data)));
            
            passFile.Data = Encoding.UTF8.GetString(ms.ToArray());
        }
    }
}