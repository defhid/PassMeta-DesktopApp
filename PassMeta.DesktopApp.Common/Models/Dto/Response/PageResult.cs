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
        /// Page number, starting from 1.
        /// </summary>
        [JsonProperty("page_num")]
        public int PageNumber { get; init; }

        /// <summary>
        /// Page size.
        /// </summary>
        [JsonProperty("page_size")]
        public int PageSize { get; init; }

        /// <summary>
        /// Total found.
        /// </summary>
        [JsonProperty("total")]
        public int Total { get; init; }

        /// <summary>
        /// List of elements.
        /// </summary>
        [JsonProperty("list")]
        public List<TElement> List { get; init; } = null!;
    }
}