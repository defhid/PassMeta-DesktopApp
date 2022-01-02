namespace PassMeta.DesktopApp.Common.Models.Entities
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Constants;
    using Newtonsoft.Json;

    /// <summary>
    /// Passfile model.
    /// </summary>
    public class PassFile
    {
        private string? _name;

        /// <summary>
        /// Passfile identifier.
        /// </summary>
        [JsonProperty("id")]
        public int Id { get; set; }

        /// <summary>
        /// Passfile name.
        /// </summary>
        [JsonProperty("name")]
        public string Name
        {
            get => _name ?? "?";
            set => _name = value;
        }

        /// <summary>
        /// Passfile distinctive color.
        /// </summary>
        [JsonProperty("color")]
        public string? Color { get; set; }
        
        /// <summary>
        /// Passfile data version.
        /// </summary>
        [JsonProperty("version")]
        public int Version { get; set; }

        /// <summary>
        /// Passfile creation date and time.
        /// </summary>
        [JsonProperty("created_on")]
        public DateTime CreatedOn { get; set; }
        
        /// <summary>
        /// Passfile information change date and time.
        /// </summary>
        [JsonProperty("info_changed_on")]
        public DateTime InfoChangedOn { get; set; }
        
        /// <summary>
        /// Passfile data change date and time.
        /// </summary>
        [JsonProperty("version_changed_on")]
        public DateTime VersionChangedOn { get; set; }

        [JsonProperty("smth")]
        // ReSharper disable once InconsistentNaming
        private string? _DataEncrypted { set => DataEncrypted = value; }
        
        /// <summary>
        /// Origin passfile, unchanged from the last download from the server. 
        /// </summary>
        [JsonProperty("__origin")]
        public PassFile? Origin { get; set; }

        /// <summary>
        /// Passfile encrypted data.
        /// </summary>
        [JsonIgnore]
        public string? DataEncrypted { get; set; }
        
        /// <summary>
        /// Passfile passphrase to decrypt <see cref="DataEncrypted"/> and encrypt <see cref="Data"/>.
        /// </summary>
        [JsonIgnore]
        public string? PassPhrase { get; set; }
        
        /// <summary>
        /// Passfile decrypted data.
        /// </summary>
        [JsonIgnore]
        public List<Section>? Data { get; set; }

        /// <summary>
        /// Passfile local problem.
        /// </summary>
        [JsonIgnore]
        public PassFileProblem? Problem { get; set; }

        #region State (one or nothing)

        /// <summary>
        /// Is passfile created locally, but not uploaded to the server?
        /// </summary>
        [JsonIgnore]
        public bool LocalCreated => Id < 1 && Origin is null;

        /// <summary>
        /// Is passfile changed locally, but not updated on the server?
        /// </summary>
        [JsonIgnore]
        public bool LocalChanged => Origin is not null;
        
        /// <summary>
        /// Is passfile deleted locally, but not deleted from the server?
        /// </summary>
        [JsonIgnore]
        public bool LocalDeleted => Id < 1 && Origin is not null;

        #endregion

        /// <inheritdoc />
        public override string ToString() => $"<Passfile Id={Id}, Name='{Name.Replace("'", "\"")}'>";

        /// <summary>
        /// Deep copy of this passfile.
        /// </summary>
        public PassFile Copy(bool copyData = true)
        {
            var clone = (PassFile)MemberwiseClone();
            clone.Data = clone.Data?.Select(section => section.Copy()).ToList();
            return clone;
        }

        /// <summary>
        /// Passfile <see cref="PassFile.Data"/> section.
        /// </summary>
        public class Section
        {
            private string? _name;
            private List<Item>? _items;

            /// <summary>
            /// Section name.
            /// </summary>
            [JsonProperty("nm")]
            public string Name { 
                get => _name ??= "?";
                set => _name = value;
            }

            /// <summary>
            /// Section items.
            /// </summary>
            [JsonProperty("it")]
            public List<Item> Items
            {
                get => _items ??= new List<Item>();
                set => _items = value;
            }

            /// <summary>
            /// Deep copy of this section.
            /// </summary>
            public Section Copy() => new()
            {
                Name = Name,
                Items = Items.Select(i => i.Copy()).ToList()
            };

            /// <summary>
            /// Passfile <see cref="PassFile.Data"/> <see cref="Section"/> item.
            /// </summary>
            public class Item
            {
                private string[]? _what;
                private string? _value;
                private string? _comment;

                /// <summary>
                /// Logins: email, phone, etc.
                /// </summary>
                [JsonProperty("wh")]
                public string[] What
                {
                    get => _what ?? Array.Empty<string>(); 
                    set => _what = value;
                }
                
                /// <summary>
                /// One password.
                /// </summary>
                [JsonProperty("pw")]
                public string Password
                {
                    get => _value ?? string.Empty; 
                    set => _value = value;
                }

                /// <summary>
                /// Some comment.
                /// </summary>
                [JsonProperty("cm")]
                public string Comment
                {
                    get => _comment ?? string.Empty; 
                    set => _comment = value;
                }

                /// <summary>
                /// Memberwise clone.
                /// </summary>
                public Item Copy() => (Item)MemberwiseClone();
            }
        }
    }
}