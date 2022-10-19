namespace PassMeta.DesktopApp.Common.Models.Settings
{
    using System.Collections.Generic;
    using System.Net;
    using Abstractions;
    using Entities;
    using Newtonsoft.Json;

    /// <summary>
    /// App context model.
    /// </summary>
    public class AppContextData
    {
        /// <inheritdoc cref="IAppContext.User"/>
        [JsonProperty("user")]
        public User? User { get; set; }

        /// <inheritdoc cref="IAppContext.Cookies"/>
        [JsonProperty("cookies")]
        public List<Cookie>? Cookies { get; set; }

        /// <inheritdoc cref="IAppContext.PassFilesCounter"/>
        [JsonProperty("pf")]
        public uint? PassFilesCounter { get; set; }

        /// <inheritdoc cref="IAppContext.ServerId"/>
        [JsonProperty("sid")]
        public string? ServerId { get; set; }
    }
}