using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace PassMeta.DesktopApp.Common.Models.Entities
{
    public class PassFileLight
    {
        [JsonProperty("id")]
        public int Id { get; set; }
        
        [JsonProperty("name")]
        public string Name { get; set; } = "";
        
        [JsonProperty("color")]
        public string? Color { get; set; }
        
        [JsonProperty("created_on")]
        public DateTime CreatedOn { get; set; }
        
        [JsonProperty("changed_on")]
        public DateTime ChangedOn { get; set; }

        [JsonProperty("version")]
        public int Version { get; set; }
        
        [JsonProperty("is_archived")]
        public bool IsArchived { get; set; }
    }
    
    public class PassFile : PassFileLight
    {
        /// Local 'changed' timestamp
        [JsonProperty("changed_local_on")]
        public DateTime? ChangedLocalOn { get; set; }
        
        [JsonProperty("smth")]
        public string? DataEncrypted { get; set; }
        
        [JsonIgnore]
        public List<Section>? Data { get; set; }

        [JsonIgnore]
        public bool IsChanged => ChangedLocalOn.HasValue;

        public class Section
        {
            private string? _name;
            
            [JsonProperty("name")]
            public string Name { 
                get => _name ?? "?";
                set => _name = value;
            }

            [JsonProperty("items")]
            public List<Item>? Items { get; set; }
            
            public Section Copy() => new()
            {
                Name = Name,
                Items = Items?.Select(i => i.Copy()).ToList()
            };

            public class Item
            {
                private string? _what;
                private string? _value;

                [JsonProperty("what")]
                public string What
                {
                    get => _what ?? "?"; 
                    set => _what = value;
                }
                
                [JsonProperty("value")]
                public string Value
                {
                    get => _value ?? "?"; 
                    set => _value = value;
                }

                public Item Copy() => new()
                {
                    What = What,
                    Value = Value
                };
            }
        }

        public void Refresh(PassFileLight passFileLight)
        {
            Id = passFileLight.Id;
            Name = passFileLight.Name;
            Color = passFileLight.Color;
            CreatedOn = passFileLight.CreatedOn;
            ChangedOn = passFileLight.ChangedOn;
            Version = passFileLight.Version;
            IsArchived = passFileLight.IsArchived;
            ChangedLocalOn = null;
        }
    }
}