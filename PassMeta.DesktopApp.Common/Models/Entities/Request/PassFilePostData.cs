using Newtonsoft.Json;

namespace PassMeta.DesktopApp.Common.Models.Entities.Request
{
    public class PassFilePostData
    {
        [JsonProperty("name")]
        public string Name { get; set; }
        
        [JsonProperty("color")]
        public string? Color { get; set; }
        
        [JsonProperty("smth")]
        public string Smth { get; set; }
        
        [JsonProperty("check_key")]
        public string CheckKey { get; set; }

        public PassFilePostData(string name, string? color, string smth, string checkKey)
        {
            Name = name;
            Color = color;
            Smth = smth;
            CheckKey = checkKey;
        }
    }
}