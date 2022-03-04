namespace PassMeta.DesktopApp.Common.Models.Dto.Response
{
    using System.Collections.Generic;
    using Newtonsoft.Json;

    /// <summary>
    /// Journal record kind DTO.
    /// </summary>
    public class JournalRecordKind
    {
        /// <summary>
        /// Kind identifier.
        /// </summary>
        [JsonProperty("id")]
        public int Id { get; init; }

        /// <summary>
        /// Names package (depending on locale).
        /// </summary>
        [JsonProperty("name")]
        public Dictionary<string, string> NamePack { get; init; } = null!;
    }
}