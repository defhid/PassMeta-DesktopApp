using System.Collections.Generic;
using Newtonsoft.Json;

namespace PassMeta.DesktopApp.Common.Models.Entities.Response
{
    public class PassMetaInfo
    {
        [JsonProperty("user")]
        public User User { get; set; }
        
        [JsonProperty("app_version")]
        public string AppVersion { get; set; }
        
        [JsonProperty("messages_translate_pack")]
        public Dictionary<string, Dictionary<string, string>> OkBadMessagesTranslatePack { get; set; }
    }
}