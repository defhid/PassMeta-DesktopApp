namespace PassMeta.DesktopApp.Common.Models.Dto.Response
{
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
        /// Kind name.
        /// </summary>
        [JsonProperty("name")]
        public string Name { get; init; } = null!;

        /// <inheritdoc />
        public override string ToString() => Name;
    }
}