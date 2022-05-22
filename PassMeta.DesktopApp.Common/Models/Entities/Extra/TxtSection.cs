namespace PassMeta.DesktopApp.Common.Models.Entities.Extra
{
    using System;
    using Newtonsoft.Json;

    /// <summary>
    /// Passfile <see cref="PassFile.TxtData"/> section.
    /// </summary>
    public class TxtSection
    {
        /// <summary>
        /// Section identifier (GUID).
        /// </summary>
        [JsonProperty("id")]
        public string Id { get; init; }

        /// <summary>
        /// Section name.
        /// </summary>
        [JsonProperty("nm")]
        public string Name { get; set; }

        /// <summary>
        /// Section items.
        /// </summary>
        [JsonProperty("it")]
        public string Content { get; set; }

        /// <summary></summary>
        public TxtSection()
        {
            Id ??= Guid.NewGuid().ToString();
            Name ??= "?";
            Content ??= "";
        }

        /// <summary>
        /// Deep copy of this section.
        /// </summary>
        public TxtSection Copy() => (TxtSection)MemberwiseClone();
    }
}