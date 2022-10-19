namespace PassMeta.DesktopApp.Common.Models
{
    using System;
    using System.Collections.Generic;
    using Abstractions.Mapping;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    
    /// <summary>
    /// PassMeta server unified response.
    /// </summary>
    public class OkBadResponse
    {
        /// <summary>
        /// Response code.
        /// </summary>
        [JsonProperty("code")]
        public int Code { get; init; }

        /// <summary>
        /// Response message.
        /// </summary>
        [JsonProperty("message")]
        public string Message { get; init; } = null!;
        
        /// <summary>
        /// If not <see cref="Success"/>, short reason.
        /// </summary>
        [JsonProperty("what")]
        public string? What { get; private set; }
        
        /// <summary>
        /// Sub-responses. Sub-error information list.
        /// </summary>
        [JsonProperty("sub")]
        public List<OkBadResponse>? Sub { get; init; }
        
        /// <summary>
        /// Additional failure information.
        /// </summary>
        [JsonProperty("more")]
        public OkBadMore? More { get; init; }

        /// <summary>
        /// Is response success?
        /// </summary>
        public bool Success => Code == 0;

        /// <summary>
        /// Replace <see cref="What"/> field values with mapped values recursively.
        /// </summary>
        public void ApplyWhatMapping(IMapper<string, string> mapper)
        {
            What = mapper.Map(What, What);
            if (Sub is null) return;
            foreach (var sub in Sub)
            {
                sub.ApplyWhatMapping(mapper);
            }
        }
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
        public TData? Data { get; init; }
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
        public string? Text { get; init; }
        
        /// <summary>
        /// Json-information.
        /// </summary>
        [JsonProperty("info")]
        public JContainer? Info { get; init; }

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

            return string.Join(Environment.NewLine, builder);
        }
    }
}