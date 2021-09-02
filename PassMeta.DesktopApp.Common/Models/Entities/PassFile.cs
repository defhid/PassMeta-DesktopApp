using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace PassMeta.DesktopApp.Common.Models.Entities
{
    public class PassFile
    {
        [JsonProperty("id")]
        public int Id { get; set; }
        
        [JsonProperty("created_on")]
        public DateTime CreatedOn { get; set; }
        
        [JsonProperty("changed_on")]
        public DateTime ChangedOn { get; set; }
        
        [JsonProperty("version")]
        public int Version { get; set; }
        
        [JsonProperty("is_archived")]
        public bool IsArchived { get; set; }
        
        [JsonProperty("data")]
        public string Data { get; set; }

        [JsonIgnore]
        public string KeyPhrase { get; set; }
    }

    public class PassFileData
    {
        [JsonProperty("name")]
        public string Name { get; set; }
            
        [JsonProperty("sections")]
        public Dictionary<string, List<Section>> Sections { get; set; }

        public class Section
        {
            [JsonProperty("what")]
            public string What { get; set; }
            
            [JsonProperty("value")]
            public string Value { get; set; }
        }
    }
}