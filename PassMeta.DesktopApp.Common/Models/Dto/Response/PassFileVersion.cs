namespace PassMeta.DesktopApp.Common.Models.Dto.Response
{
    using System;
    using Newtonsoft.Json;

    /// <summary>
    /// Passfile version info.
    /// </summary>
    public class PassFileVersion
    {
        ///
        [JsonProperty("version")]
        public int Version { get; set; }

        ///
        [JsonProperty("version_date")]
        public DateTime VersionDate { get; set; }
    }
}