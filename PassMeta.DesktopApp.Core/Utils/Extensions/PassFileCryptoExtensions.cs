namespace PassMeta.DesktopApp.Core.Utils.Extensions
{
    using System;
    using System.Collections.Generic;
    using Common;
    using Common.Enums;
    using Common.Interfaces;
    using Common.Interfaces.Services;
    using Common.Models;
    using Common.Models.Entities;
    using Common.Models.Entities.Extra;
    using Newtonsoft.Json;

    /// <summary>
    /// Extension crypto-methods for <see cref="PassFile"/>.
    /// </summary>
    public static class PassFileCryptoExtensions
    {
        private static ILogService Logger => EnvironmentContainer.Resolve<ILogService>();
        private static IDetailedResult DecryptionError => Result.Failure(Resources.PASSFILE__DECRYPTION_ERROR);
        private static IDetailedResult EncryptionError => Result.Failure(Resources.PASSFILE__ENCRYPTION_ERROR);

        /// <summary>
        /// Decrypts <see cref="PassFile.DataEncrypted"/> and returns result.
        /// </summary>
        /// <param name="passFile">Passfile which data to decrypt.</param>
        /// <param name="passPhrase">Phrase to use for decryption.</param>
        /// <param name="silent">Not to write failure logs.</param>
        /// <remarks>
        /// <see cref="PassFile.PassPhrase"/> must be not null.
        /// </remarks>
        public static IDetailedResult Decrypt(this PassFile passFile, string? passPhrase = null, bool silent = false)
        {
            var originPassPhrase = passFile.PassPhrase;
            if (passPhrase is not null)
            {
                passFile.PassPhrase = passPhrase;
            }
            
            if (string.IsNullOrEmpty(passFile.PassPhrase))
            {
                passFile.PassPhrase = originPassPhrase;
                Logger.Error("Using Decrypt method without key phrase!");
                return DecryptionError;
            }
            
            if (string.IsNullOrEmpty(passFile.DataEncrypted))
            {
                passFile.PassPhrase = originPassPhrase;
                Logger.Error("Using Decrypt method without encrypted data!");
                return DecryptionError;
            }
            
            var service = EnvironmentContainer.Resolve<ICryptoService>();

            var content = service.Decrypt(passFile.DataEncrypted, passFile.PassPhrase, silent);
            if (content is null)
            {
                passFile.PassPhrase = originPassPhrase;
                return Result.Failure(Resources.PASSFILE__VALIDATION__WRONG_PASSPHRASE);
            }

            try
            {
                switch (passFile.Type)
                {
                    case PassFileType.Pwd:
                        passFile.PwdData = JsonConvert.DeserializeObject<List<PwdSection>>(content) ?? new List<PwdSection>();
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(passFile.Type), passFile.Type, null);
                }
            }
            catch (Exception ex)
            {
                passFile.PassPhrase = originPassPhrase;
                Logger.Error(ex, "Passfile deserializing");
                return DecryptionError;
            }
            
            return Result.Success();
        }
        
        /// <summary>
        /// Encrypts <see cref="PassFile.PwdData"/> and sets result to <see cref="PassFile.DataEncrypted"/>.
        /// </summary>
        /// <remarks>
        /// <see cref="PassFile.PassPhrase"/> must be not null.
        /// </remarks>
        public static IDetailedResult Encrypt(this PassFile passFile)
        {
            if (string.IsNullOrEmpty(passFile.PassPhrase))
            {
                Logger.Error("Using Encrypt method without key phrase!");
                return EncryptionError;
            }

            if (passFile.PwdData is null)
            {
                Logger.Error("Using Encrypt method without decrypted data!");
                return EncryptionError;
            }

            string data;
            try
            {
                switch (passFile.Type)
                {
                    case PassFileType.Pwd:
                        data = JsonConvert.SerializeObject(passFile.PwdData);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(passFile.Type), passFile.Type, null);
                }
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