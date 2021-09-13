using System;
using Newtonsoft.Json;
using PassMeta.DesktopApp.Common;
using PassMeta.DesktopApp.Common.Interfaces.Services;
using PassMeta.DesktopApp.Common.Models.Entities;
using Splat;

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
            
            var service = Locator.Current.GetService<ICryptoService>();
            var content = service!.Decrypt(passFile.Data, passFile.KeyPhrase);

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

            var service = Locator.Current.GetService<ICryptoService>();
            passFile.Data = service!.Encrypt(JsonConvert.SerializeObject(data), passFile.KeyPhrase);
        }
    }
}