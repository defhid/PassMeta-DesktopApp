namespace PassMeta.DesktopApp.Core.Utils.Extensions
{
    using System;
    using System.Collections.Generic;
    using Common;
    using Common.Interfaces.Services;
    using Common.Models;
    using Common.Models.Entities;
    using Newtonsoft.Json;

    /// <summary>
    /// Extension crypto-methods for <see cref="PassFile"/>.
    /// </summary>
    public static class PassFileCryptoExtensions
    {
        private static ILogService Logger => EnvironmentContainer.Resolve<ILogService>();
        private static Result DecryptionError => Result.Failure(Resources.PASSFILE__DECRYPTION_ERROR);
        private static Result EncryptionError => Result.Failure(Resources.PASSFILE__ENCRYPTION_ERROR);

        /// <summary>
        /// Decrypts <see cref="PassFile.DataEncrypted"/> and returns result.
        /// </summary>
        /// <remarks>
        /// <see cref="PassFile.PassPhrase"/> must be not null.
        /// </remarks>
        public static Result Decrypt(this PassFile passFile)
        {
            if (string.IsNullOrEmpty(passFile.PassPhrase))
            {
                Logger.Error("Using Decrypt method without key phrase!");
                return DecryptionError;
            }
            
            if (string.IsNullOrEmpty(passFile.DataEncrypted))
            {
                Logger.Error("Using Decrypt method without encrypted data!");
                return DecryptionError;
            }
            
            var service = EnvironmentContainer.Resolve<ICryptoService>();

            var content = service.Decrypt(passFile.DataEncrypted, passFile.PassPhrase);
            if (content is null)
                return Result.Failure(Resources.PASSFILE__VALIDATION__WRONG_PASSPHRASE);

            try
            {
                passFile.Data = JsonConvert.DeserializeObject<List<PassFile.Section>>(content) ?? new List<PassFile.Section>();
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Passfile deserializing");
                return DecryptionError;
            }
            
            return Result.Success();
        }
        
        /// <summary>
        /// Encrypts <see cref="PassFile.Data"/> and sets result to <see cref="PassFile.DataEncrypted"/>.
        /// </summary>
        /// <remarks>
        /// <see cref="PassFile.PassPhrase"/> must be not null.
        /// </remarks>
        public static Result Encrypt(this PassFile passFile)
        {
            if (string.IsNullOrEmpty(passFile.PassPhrase))
            {
                Logger.Error("Using Encrypt method without key phrase!");
                return EncryptionError;
            }

            if (passFile.Data is null)
            {
                Logger.Error("Using Encrypt method without decrypted data!");
                return EncryptionError;
            }

            string data;
            try
            {
                data = JsonConvert.SerializeObject(passFile.Data);
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Passfile deserializing");
                return EncryptionError;
            }
            
            var service = EnvironmentContainer.Resolve<ICryptoService>();
            
            passFile.DataEncrypted = service.Encrypt(data, passFile.PassPhrase);
            if (passFile.DataEncrypted is null)
            {
                return EncryptionError;
            }

            return Result.Success();
        }
    }
}