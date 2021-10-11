using Newtonsoft.Json;

namespace PassMeta.DesktopApp.Common.Models.Entities.Request
{
    public class PassFIlePostData
    {
        [JsonProperty("name")]
        public string Name { get; set; }
        
        [JsonProperty("color")]
        public string? Color { get; set; }
        
        [JsonProperty("smth")]
        public string Smth { get; set; }

        public PassFIlePostData(string name, string? color, string smth)
        {
            Name = name;
            Color = color;
            Smth = smth;
        }
    }
}