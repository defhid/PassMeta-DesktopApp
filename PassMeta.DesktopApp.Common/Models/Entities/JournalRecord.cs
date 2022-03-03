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
        /// Record kind identifier.
        /// </summary>
        [JsonProperty("kind_id")]
        public int KindId { get; set; }
        
        /// <summary>
        /// Login of the user that performed the recorded request.
        /// </summary>
        [JsonProperty("user_login")]
        public string? UserLogin { get; init; }

        /// <summary>
        /// Additional information.
        /// </summary>
        [JsonProperty("more")]
        public string? More { get; init; }
        
        /// <summary>
        /// Record date and time.
        /// </summary>
        [JsonProperty("timestamp")]
        public DateTime TimeStamp { get; init; }
    }
}