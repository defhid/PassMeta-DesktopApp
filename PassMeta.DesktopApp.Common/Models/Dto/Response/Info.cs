#pragma warning disable 8618
namespace PassMeta.DesktopApp.Common.Models.Dto.Response
{
    using System.Collections.Generic;
    using Entities;
    using Newtonsoft.Json;

    /// <summary>
    /// PassMeta server response when starting the application.
    /// </summary>
    public class PassMetaInfo
    {
        /// <summary>
        /// Current user.
        /// </summary>
        [JsonProperty("user")]
        public User? User { get; set; }
        
        /// <summary>
        /// Server version.
        /// </summary>
        [JsonProperty("app_version")]
        public string? AppVersion { get; set; }
        
        /// <summary>
        /// <see cref="OkBadResponse"/> message translation package.
        /// </summary>
        [JsonProperty("messages_translate_pack")]
        public Dictionary<string, Dictionary<string, string>>? OkBadMessagesTranslatePack { get; set; }
    }
}