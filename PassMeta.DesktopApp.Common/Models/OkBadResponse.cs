#pragma warning disable 8618
namespace PassMeta.DesktopApp.Common.Models
{
    using System;
    using System.Collections.Generic;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    
    /// <summary>
    /// PassMeta server unified response.
    /// </summary>
    public class OkBadResponse
    {
        /// <summary>
        /// Response message.
        /// </summary>
        [JsonProperty("message")]
        public string Message { get; set; }
        
        /// <summary>
        /// If not <see cref="Success"/>, short reason.
        /// </summary>
        [JsonProperty("what")]
        public string? What { get; set; }
        
        /// <summary>
        /// Sub-responses. Sub-error information list.
        /// </summary>
        [JsonProperty("sub")]
        public List<OkBadResponse>? Sub { get; set; }
        
        /// <summary>
        /// Additional failure information.
        /// </summary>
        [JsonProperty("more")]
        public OkBadMore? More { get; set; }

        /// <summary>
        /// Is response success?
        /// </summary>
        public bool Success => Message == "OK";
    }

    /// <summary>
    /// <see cref="OkBadResponse"/> with data.
    /// </summary>
    public class OkBadResponse<TData> : OkBadResponse
    {
        /// <summary>
        /// If <see cref="OkBadResponse.Success"/>, response data.
        /// </summary>
        [JsonProperty("data")]
        public TData? Data { get; set; }
    }
    
    /// <summary>
    /// <see cref="OkBadResponse"/> additional information.
    /// </summary>
    public class OkBadMore
    {
        /// <summary>
        /// Some text message.
        /// </summary>
        [JsonProperty("text")]
        public string? Text { get; set; }
        
        /// <summary>
        /// Json-information.
        /// </summary>
        [JsonProperty("info")]
        public JObject? Info { get; set; }
        
        /// <summary>
        /// Allowed values.
        /// </summary>
        [JsonProperty("allowed")]
        public JArray? Allowed { get; set; }
        
        /// <summary>
        /// Disallowed values.
        /// </summary>
        [JsonProperty("disallowed")]
        public JArray? Disallowed { get; set; }

        /// <summary>
        /// Required fields/values.
        /// </summary>
        [JsonProperty("required")]
        public JArray? Required { get; set; }

        /// <summary>
        /// Minimum valid value.
        /// </summary>
        [JsonProperty("min_allowed")]
        public object? MinAllowed { get; set; }
        
        /// <summary>
        /// Maximum valid value.
        /// </summary>
        [JsonProperty("max_allowed")]
        public object? MaxAllowed { get; set; }

        /// <summary>
        /// Build string with information from notnull fields.
        /// </summary>
        public override string ToString()
        {
            var builder = new List<string>();
            
            if (Text is not null)
                builder.Add(Text);
            if (Info is not null) 
                builder.Add($"{Resources.OKBAD_MORE__INFO}: {Info}");
            if (Allowed is not null) 
                builder.Add($"{Resources.OKBAD_MORE__ALLOWED}: {string.Join(", ", Allowed)}");
            if (Disallowed is not null) 
                builder.Add($"{Resources.OKBAD_MORE__DISALLOWED}: {string.Join(", ", Disallowed)}");
            if (Required is not null) 
                builder.Add($"{Resources.OKBAD_MORE__REQUIRED}: {string.Join(", ", Required)}");
            if (MinAllowed is not null) 
                builder.Add($"{Resources.OKBAD_MORE__MIN_ALLOWED}: {MinAllowed}");
            if (MaxAllowed is not null) 
                builder.Add($"{Resources.OKBAD_MORE__MAX_ALLOWED}: {MaxAllowed}");

            return string.Join(Environment.NewLine, builder);
        }
    }
}