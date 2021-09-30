using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using PassMeta.DesktopApp.Common;
using PassMeta.DesktopApp.Common.Interfaces.Services;
using PassMeta.DesktopApp.Common.Models.Entities;
using PassMeta.DesktopApp.Core.Utils;
using Splat;

namespace PassMeta.DesktopApp.Core.Extensions
{
    public static class PassFileCryptoExtensions
    {
        /// <summary>
        /// Decrypts <see cref="PassFile.DataEncrypted"/> and returns result.
        /// </summary>
        /// <remarks>
        /// <see cref="PassFile.KeyPhrase"/> must be not null.
        /// </remarks>
        public static void Decrypt(this PassFile passFile)
        {
            if (string.IsNullOrEmpty(AppConfig.Current.PassFilesKeyPhrase))
                throw new NullReferenceException("Using Decrypt method without key phrase!");
            
            var service = Locator.Current.GetService<ICryptoService>();
            var content = service!.Decrypt(passFile.DataEncrypted, AppConfig.Current.PassFilesKeyPhrase);

            try
            {
                passFile.Data = JsonConvert.DeserializeObject<List<PassFile.Section>>(content);
            }
            catch
            {
                throw new FormatException(Resources.ERR__PASSFILE_DECRYPTION);
            }
        }
        
        /// <summary>
        /// Encrypts <paramref name="passFile.Data"/> and sets result to <see cref="PassFile.DataEncrypted"/>.
        /// </summary>
        /// <remarks>
        /// <see cref="PassFile.KeyPhrase"/> must be not null.
        /// </remarks>
        public static void Encrypt(this PassFile passFile)
        {
            if (string.IsNullOrEmpty(AppConfig.Current.PassFilesKeyPhrase))
                throw new NullReferenceException("Using Encrypt method without key phrase!");

            var service = Locator.Current.GetService<ICryptoService>();
            passFile.DataEncrypted = service!.Encrypt(JsonConvert.SerializeObject(passFile.Data), 
                AppConfig.Current.PassFilesKeyPhrase);
        }
    }
}