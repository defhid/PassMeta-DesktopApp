namespace PassMeta.DesktopApp.Common.Models.Dto.Response
{
    using System.Collections.Generic;
    using Newtonsoft.Json;

    /// <summary>
    /// Page result with list of <typeparamref name="TElement"/>.
    /// </summary>
    public class PageResult<TElement>
    {
        /// <summary>
        /// Total found.
        /// </summary>
        [JsonProperty("total")]
        public int Total { get; init; }
        
        /// <summary>
        /// Page offset, in records.
        /// </summary>
        [JsonProperty("offset")]
        public int Offset { get; init; }
        
        /// <summary>
        /// Page limit.
        /// </summary>
        [JsonProperty("limit")]
        public int Limit { get; init; }

        /// <summary>
        /// List of elements.
        /// </summary>
        [JsonProperty("list")]
        public List<TElement> List { get; init; } = null!;
    }
}