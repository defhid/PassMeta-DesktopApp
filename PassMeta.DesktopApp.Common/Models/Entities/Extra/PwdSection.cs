namespace PassMeta.DesktopApp.Common.Models.Entities.Extra
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Newtonsoft.Json;

    /// <summary>
    /// Passfile <see cref="PassFile.PwdData"/> section.
    /// </summary>
    public class PwdSection
    {
        private string? _search;  // TODO: remove
        
        /// <summary>
        /// Section identifier (GUID).
        /// </summary>
        [JsonProperty("id")]
        public string Id { get; init; }

        /// <summary>
        /// Section name.
        /// </summary>
        [JsonProperty("nm")]
        public string Name { get; set; }

        /// <summary>
        /// Section items.
        /// </summary>
        [JsonProperty("it")]
        public List<PwdItem> Items { get; set; }

        /// <summary>
        /// Prepared value for search.
        /// </summary>
        [JsonIgnore]
        public string Search => _search ??= Name.Trim().ToLower();

        /// <summary></summary>
        public PwdSection()
        {
            Id ??= Guid.NewGuid().ToString();
            Name ??= "?";
            Items ??= new List<PwdItem>();
        }

        /// <summary>
        /// Deep copy of this section.
        /// </summary>
        public PwdSection Copy() => new()
        {
            Id = Id,
            Name = Name,
            Items = Items.Select(i => i.Copy()).ToList()
        };
    }
}