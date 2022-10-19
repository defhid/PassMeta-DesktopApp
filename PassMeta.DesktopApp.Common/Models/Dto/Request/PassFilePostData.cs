namespace PassMeta.DesktopApp.Common.Models.Dto.Request
{
    using System;
    using Newtonsoft.Json;

    /// <summary>
    /// Passfile creation data.
    /// </summary>
    public class PassFilePostData
    {
        ///
        [JsonProperty("name")]
        public string Name { get; init; } = null!;
        
        ///
        [JsonProperty("color")]
        public string? Color { get; init; }
        
        ///
        [JsonProperty("type_id")]
        public int TypeId { get; init; }
        
        ///
        [JsonProperty("created_on")]
        public DateTime CreatedOn { get; init; }
        
        ///
        [JsonProperty("smth")]
        public string Smth { get; init; } = null!;
    }
}