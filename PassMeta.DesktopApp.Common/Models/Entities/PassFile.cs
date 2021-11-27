namespace PassMeta.DesktopApp.Common.Models.Entities
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Newtonsoft.Json;
    
    public class PassFileLight
    {
        [JsonProperty("id")]
        public int Id { get; set; }
        
        [JsonProperty("name")]
        public string Name { get; set; } = string.Empty;
        
        [JsonProperty("color")]
        public string? Color { get; set; }
        
        [JsonProperty("check_key")]
        public string CheckKey { get; set; } = string.Empty;
        
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
        public string? PassPhrase { get; set; }
        
        [JsonIgnore]
        public PassFileProblem? Problem { get; set; }
        
        [JsonIgnore]
        public bool HasProblem => Problem is not null;

        [JsonIgnore]
        public bool IsLocalChanged => ChangedLocalOn.HasValue;
        
        [JsonIgnore]
        public bool IsDecrypted => Data is not null;

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
                private string[]? _what;
                private string? _value;
                private string? _comment;

                [JsonProperty("what")]
                public string[] What
                {
                    get => _what ?? Array.Empty<string>(); 
                    set => _what = value;
                }
                
                [JsonProperty("pass")]
                public string Password
                {
                    get => _value ?? string.Empty; 
                    set => _value = value;
                }

                [JsonProperty("com")]
                public string Comment
                {
                    get => _comment ?? string.Empty; 
                    set => _comment = value;
                }

                public Item Copy() => new()
                {
                    What = What,
                    Password = Password,
                    Comment = Comment
                };
            }
        }

        public PassFile Copy()
        {
            var copy = (PassFile)MemberwiseClone();
            copy.Data = copy.Data?.Select(section => section.Copy()).ToList();
            return copy;
        }

        public void Refresh(PassFileLight passFileLight)
        {
            Id = passFileLight.Id;
            Name = passFileLight.Name;
            Color = passFileLight.Color;
            CheckKey = passFileLight.CheckKey;
            CreatedOn = passFileLight.CreatedOn;
            ChangedOn = passFileLight.ChangedOn;
            Version = passFileLight.Version;
            IsArchived = passFileLight.IsArchived;
            ChangedLocalOn = null;
        }
    }
}