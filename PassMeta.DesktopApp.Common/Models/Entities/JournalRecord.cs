namespace PassMeta.DesktopApp.Common.Models.Entities
{
    using System;
    using Newtonsoft.Json;

    /// <summary>
    /// Record from remote journal.
    /// </summary>
    public class JournalRecord
    {
        /// <summary>
        /// Record identifier.
        /// </summary>
        [JsonProperty("id")]
        public int Id { get; init; }

        /// <summary>
        /// Record kind.
        /// </summary>
        [JsonProperty("kind")]
        public string Kind { get; set; } = null!;

        /// <summary>
        /// Ip of the user that performed the recorded request.
        /// </summary>
        [JsonProperty("user_ip")]
        public string UserIp { get; init; } = null!;

        /// <summary>
        /// Login of the user that performed the recorded request.
        /// </summary>
        [JsonProperty("user_login")]
        public string? UserLogin { get; init; }
        
        /// <summary>
        /// Affected passfile id.
        /// </summary>
        [JsonProperty("affected_passfile_id")]
        public long? PassFileId { get; init; }

        /// <summary>
        /// Affected passfile name.
        /// </summary>
        [JsonProperty("affected_passfile_name")]
        public string? PassFileName { get; init; }

        /// <summary>
        /// Additional information.
        /// </summary>
        [JsonProperty("more")]
        public string More { get; init; } = null!;
        
        /// <summary>
        /// Record date and time.
        /// </summary>
        [JsonProperty("timestamp")]
        public DateTime TimeStamp { get; init; }
    }
}