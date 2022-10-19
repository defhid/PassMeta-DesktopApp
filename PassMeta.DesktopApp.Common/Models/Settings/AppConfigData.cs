namespace PassMeta.DesktopApp.Common.Models.Settings
{
    using Abstractions;
    using Newtonsoft.Json;

    /// <summary>
    /// App configuration model.
    /// </summary>
    public class AppConfigData
    {
        /// <inheritdoc cref="IAppConfig.ServerUrl"/>
        [JsonProperty("server")]
        public string? ServerUrl { get; set; }
        
        /// <inheritdoc cref="IAppConfig.CultureCode"/>
        [JsonProperty("culture")]
        public string? CultureCode { get; set; }

        /// <inheritdoc cref="IAppConfig.HidePasswords"/>
        [JsonProperty("hide_pwd")]
        public bool? HidePasswords { get; set; }

        /// <inheritdoc cref="IAppConfig.DevMode"/>
        [JsonProperty("dev")]
        public bool? DevMode { get; set; }

        /// <inheritdoc cref="IAppConfig.DefaultPasswordLength"/>
        [JsonProperty("default_password_length")]
        public int? DefaultPasswordLength { get; set; }
    }
}