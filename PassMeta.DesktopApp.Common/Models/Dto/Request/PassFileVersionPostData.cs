namespace PassMeta.DesktopApp.Common.Models.Dto.Request
{
    using Newtonsoft.Json;

    /// <summary>
    /// Passfile new version data.
    /// </summary>
    public class PassFileVersionPostData
    {
        ///
        [JsonProperty("smth")]
        public string DataEncrypted { get; init; } = null!;
    }
}