using Newtonsoft.Json;
using PassMeta.DesktopApp.Common.Abstractions;

namespace PassMeta.DesktopApp.Common.Models.Settings;

/// <summary>
/// Application config DTO.
/// </summary>
public class AppConfigDto
{
    /// <inheritdoc cref="IAppConfig.ServerUrl"/>
    [JsonProperty("server")]
    public string? ServerUrl { get; set; }
        
    /// <inheritdoc cref="IAppConfig.Culture"/>
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