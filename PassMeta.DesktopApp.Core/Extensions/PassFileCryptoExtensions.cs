namespace PassMeta.DesktopApp.Core.Extensions
{
    using DesktopApp.Common;
    using DesktopApp.Common.Interfaces.Services;
    using DesktopApp.Common.Models;
    using DesktopApp.Common.Models.Entities;
    
    using System;
    using System.Collections.Generic;
    using Newtonsoft.Json;
    using Splat;
    
    public static class PassFileCryptoExtensions
    {
        /// <summary>
        /// Decrypts <see cref="PassFile.DataEncrypted"/> and returns result.
        /// </summary>
        /// <remarks>
        /// <see cref="PassFile.PassPhrase"/> must be not null.
        /// </remarks>
        public static Result Decrypt(this PassFile passFile)
        {
            if (string.IsNullOrEmpty(passFile.PassPhrase))
                throw new NullReferenceException("Using Decrypt method without key phrase!");
            
            if (string.IsNullOrEmpty(passFile.DataEncrypted))
                throw new NullReferenceException("Using Decrypt method without encrypted data!");
            
            var service = Locator.Current.GetService<ICryptoService>()!;
            if (service.MakeCheckKey(passFile.PassPhrase) != passFile.CheckKey)
            {
                return new Result(false, Resources.PASSFILE__WRONG_PASSPHRASE);
            }
            
            var content = service.Decrypt(passFile.DataEncrypted, passFile.PassPhrase);

            try
            {
                passFile.Data = JsonConvert.DeserializeObject<List<PassFile.Section>>(content) 
                                ?? new List<PassFile.Section>();
                passFile.CheckKey = service.MakeCheckKey(passFile.PassPhrase);
                return Result.Success();
            }
            catch
            {
                return new Result(false, Resources.PASSFILE__DECRYPTION_ERROR);
            }
        }
        
        /// <summary>
        /// Encrypts <paramref name="passFile.Data"/> and sets result to <see cref="PassFile.DataEncrypted"/>.
        /// </summary>
        /// <remarks>
        /// <see cref="PassFile.PassPhrase"/> must be not null.
        /// </remarks>
        public static void Encrypt(this PassFile passFile)
        {
            if (string.IsNullOrEmpty(passFile.PassPhrase))
                throw new NullReferenceException("Using Encrypt method without key phrase!");
            
            if (passFile.Data is null)
                throw new NullReferenceException("Using Encrypt method without decrypted data!");

            var service = Locator.Current.GetService<ICryptoService>()!;
            
            passFile.DataEncrypted = service.Encrypt(
                JsonConvert.SerializeObject(passFile.Data ?? new List<PassFile.Section>()), passFile.PassPhrase);
            
            passFile.CheckKey = service.MakeCheckKey(passFile.PassPhrase);
        }
    }
}