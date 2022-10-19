namespace PassMeta.DesktopApp.Core.Services
{
    using DesktopApp.Core.Utils;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Security.Cryptography;
    using Common.Abstractions.Services;

    /// <inheritdoc />
    public class CryptoService : ICryptoService
    {
        private const string FailureGenerationResult = ":(";

        private static readonly Random Random = new();

        private readonly ILogService _logger = EnvironmentContainer.Resolve<ILogService>();

        /// <inheritdoc />
        public string? Encrypt(string data, string keyPhrase)
        {
            try
            {
                var encryption = PassFileConvention.JsonEncoding.GetBytes(data);
                
                using (var aes = Aes.Create())
                {
                    aes.IV = PassFileConvention.Encryption.Salt;
                
                    for (var i = 0; i < PassFileConvention.Encryption.CryptoK; ++i)
                    {
                        aes.Key = PassFileConvention.Encryption.MakeKey(i, keyPhrase);

                        using var encryptor = aes.CreateEncryptor();
                        using var ms = new MemoryStream();
                        using (var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
                        {
                            cs.Write(encryption, 0, encryption.Length);
                            cs.Flush();
                        }

                        encryption = ms.ToArray();
                    }
                }

                return PassFileConvention.Convert.EncryptedBytesToString(encryption);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Encryption failed");
                return null;
            }
        }

        /// <inheritdoc />
        public string? Decrypt(string data, string keyPhrase, bool silent = false)
        {
            byte[] dataBytes;
            
            try
            {
                dataBytes = PassFileConvention.Convert.EncryptedStringToBytes(data);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Decryption failed");
                return null;
            }
            
            return Decrypt(dataBytes, keyPhrase, silent);
        }

        /// <inheritdoc />
        public string? Decrypt(byte[] data, string keyPhrase, bool silent = false)
        {
            try
            {
                using (var aes = Aes.Create())
                {
                    aes.IV = PassFileConvention.Encryption.Salt;

                    for (var i = PassFileConvention.Encryption.CryptoK - 1; i >= 0; --i)
                    {
                        aes.Key = PassFileConvention.Encryption.MakeKey(i, keyPhrase);

                        using var decryptor = aes.CreateDecryptor();
                        using var ms = new MemoryStream(data);
                        using var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read);
                        using var msResult = new MemoryStream();

                        cs.CopyTo(msResult);
                        data = msResult.ToArray();
                    }
                }

                return PassFileConvention.JsonEncoding.GetString(data);
            }
            catch (CryptographicException ex)
            {
                if (!silent)
                    _logger.Warning("Decryption failed: " + ex.Message);
                return null;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Decryption failed");
                return null;
            }
        }

        /// <inheritdoc />
        public string GeneratePassword(int length, bool digits, bool lowercase, bool uppercase, bool special)
        {
            const string userFriendlyLowercaseSet = "abcdefghijkmnopqrstuvwxyz";
            const string userFriendlyUppercaseSet = "ABCDEFGHJKLMNPQRSTUVWXYZ";
            const string userFriendlySet = userFriendlyLowercaseSet + userFriendlyUppercaseSet;
            const string digitSet = "0123456789";
            const string specialSet = "*-_!@";
            
            var builder = Enumerable.Repeat(string.Empty, 0);

            var letters = lowercase || uppercase;
            var k = (float) Math.Max((lowercase ? 1 : 0) + (uppercase ? 1 : 0), 1) / 2;

            if (!digits && !letters && !special)
            {
                return FailureGenerationResult;
            }

            try
            {
                if (digits)
                    builder = builder.Concat(Enumerable.Repeat(digitSet, (int) (5 * k)));

                if (lowercase)
                    builder = builder.Concat(Enumerable.Repeat(userFriendlyLowercaseSet, 2));

                if (uppercase)
                    builder = builder.Concat(Enumerable.Repeat(userFriendlyUppercaseSet, 2));

                if (special)
                    builder = builder.Concat(Enumerable.Repeat(specialSet, (int) (5 * k)));

                var chars = string.Concat(builder.OrderBy(_ => Random.Next()));

                var stack = new Stack<char>(length);

                var result = new string(Enumerable.Repeat(chars, length * 20)
                    .Select(s => s[Random.Next(s.Length)])
                    .SkipWhile(letters
                        ? s => !userFriendlySet.Contains(s)
                        : _ => false)
                    .Where(letters || digits
                        ? s =>
                        {
                            if (stack.TryPeek(out var prev) && specialSet.Contains(prev) && specialSet.Contains(s))
                            {
                                return false;
                            }

                            stack.Push(s);
                            return true;
                        }
                        : _ => true)
                    .Take(length)
                    .ToArray());

                return result.Length < length
                    ? GeneratePassword(length, digits, lowercase, uppercase, special) 
                    : result;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Password generation failed");
                return FailureGenerationResult;
            }
        }
    }
}