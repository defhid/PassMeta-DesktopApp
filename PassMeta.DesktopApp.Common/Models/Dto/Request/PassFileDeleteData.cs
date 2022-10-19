namespace PassMeta.DesktopApp.Common.Models.Dto.Request
{
    using Newtonsoft.Json;

    /// <summary>
    /// Passfile deletion data.
    /// </summary>
    public class PassFileDeleteData
    {
        ///
        [JsonProperty("check_password")]
        public string CheckPassword { get; init; } = null!;
    }
}