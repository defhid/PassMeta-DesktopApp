namespace PassMeta.DesktopApp.Common.Models.Dto.Request
{
    using Newtonsoft.Json;

    /// <summary>
    /// Passfile information patch data.
    /// </summary>
    public class PassFileInfoPatchData
    {
        ///
        [JsonProperty("name")]
        public string Name { get; init; } = null!;
        
        ///
        [JsonProperty("color")]
        public string? Color { get; init; }
    }
}