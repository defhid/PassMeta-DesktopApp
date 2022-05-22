namespace PassMeta.DesktopApp.Core.Utils
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Security.Cryptography;
    using System.Text;
    using Common.Models.Entities.Extra;
    using Newtonsoft.Json;

    /// <summary>
    /// Rules for working with passfiles.
    /// </summary>
    public static class PassFileConvention
    {
        /// <summary>
        /// Encoding for passfile's data.
        /// </summary>
        public static readonly Encoding JsonEncoding = new UTF8Encoding(false);

        /// <summary>
        /// Statics for converting passfile data.
        /// </summary>
        public static class Convert
        {
            /// <summary>
            /// Convert encrypted passfile data from bytes to string.
            /// </summary>
            [return: NotNullIfNotNull("dataBytes")]
            public static string? EncryptedBytesToString(byte[]? dataBytes) 
                => dataBytes is null ? null : System.Convert.ToBase64String(dataBytes);
        
            /// <summary>
            /// Convert encrypted passfile data from string to bytes.
            /// </summary>
            [return: NotNullIfNotNull("dataString")]
            public static byte[]? EncryptedStringToBytes(string? dataString) 
                => dataString is null ? null : System.Convert.FromBase64String(dataString);

            /// <summary>
            /// Convert decrypted json data to raw sections list.
            /// </summary>
            public static List<PwdSection> ToRaw(string decryptedJson)
                => JsonConvert.DeserializeObject<List<PwdSection>>(decryptedJson, JsonSettings) ?? new List<PwdSection>();

            /// <summary>
            /// Convert raw sections list to decrypted json data.
            /// </summary>
            public static string FromRaw(List<PwdSection> sections, bool indented = false)
                => JsonConvert.SerializeObject(sections, indented ? JsonIndentedSettings : JsonSettings);

            private static readonly JsonSerializerSettings JsonSettings = new()
            {
                MissingMemberHandling = MissingMemberHandling.Ignore,
                NullValueHandling = NullValueHandling.Ignore,
                Formatting = Formatting.None
            };
            
            private static readonly JsonSerializerSettings JsonIndentedSettings = new()
            {
                MissingMemberHandling = MissingMemberHandling.Ignore,
                NullValueHandling = NullValueHandling.Ignore,
                Formatting = Formatting.Indented
            };
        }
        
        /// <summary>
        /// Statics for encryption/decryption passfile data.
        /// </summary>
        public static class Encryption
        {
            /// <summary>
            /// Number of encryption iterations.
            /// </summary>
            public const int CryptoK = 100;
            
            /// <summary>
            /// Passfiles encryption salt.
            /// </summary>
            public static readonly byte[] Salt = Encoding.UTF8.GetBytes("PassMetaFileSalt");

            /// <summary>
            /// Make encryption/decryption key for specific iteration by keyphrase.
            /// </summary>
            public static byte[] MakeKey(int iteration, string keyPhrase)
            {
                var offset = (CryptoK + iteration) % keyPhrase.Length;
                var key = keyPhrase[..offset] + Math.Pow(CryptoK - iteration, iteration % 5) + keyPhrase[offset..];
                        
                return SHA256.HashData(Encoding.Unicode.GetBytes(key));
            }
        }
    }
}