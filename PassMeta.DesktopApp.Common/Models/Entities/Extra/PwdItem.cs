namespace PassMeta.DesktopApp.Common.Models.Entities.Extra
{
    using System;
    using Newtonsoft.Json;

    /// <summary>
    /// Passfile <see cref="PassFile.PwdData"/> <see cref="PwdSection"/> item.
    /// </summary>
    public class PwdItem
    {
        private string? _search;
                
        /// <summary>
        /// Logins: email, phone, etc.
        /// </summary>
        [JsonProperty("wh")]
        public string[] What { get; set; }

        /// <summary>
        /// One password.
        /// </summary>
        [JsonProperty("pw")]
        public string Password { get; set; }

        /// <summary>
        /// Some comment.
        /// </summary>
        [JsonProperty("cm")]
        public string Comment { get; set; }
                
        /// <summary>
        /// Prepared value for search.
        /// </summary>
        [JsonIgnore]
        public string Search => _search ??= Comment.Trim().ToLower();

        /// <summary></summary>
        public PwdItem()
        {
            What ??= Array.Empty<string>();
            Password ??= string.Empty;
            Comment ??= string.Empty;
        }

        /// <summary>
        /// Memberwise clone.
        /// </summary>
        public PwdItem Copy() => (PwdItem)MemberwiseClone();
    }
}