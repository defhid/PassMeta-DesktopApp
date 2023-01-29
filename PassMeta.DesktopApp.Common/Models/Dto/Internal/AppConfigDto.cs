using System.Text.Json.Serialization;
using PassMeta.DesktopApp.Common.Abstractions.AppConfig;

namespace PassMeta.DesktopApp.Common.Models.Dto.Internal;

/// <summary>
/// Application config DTO.
/// </summary>
public class AppConfigDto
{
    /// <inheritdoc cref="IAppConfig.ServerUrl"/>
    [JsonPropertyName("server")]
    public string? ServerUrl { get; set; }
        
    /// <inheritdoc cref="IAppConfig.Culture"/>
    [JsonPropertyName("culture")]
    public string? CultureCode { get; set; }

    /// <inheritdoc cref="IAppConfig.HidePasswords"/>
    [JsonPropertyName("hide_pwd")]
    public bool? HidePasswords { get; set; }

    /// <inheritdoc cref="IAppConfig.DevMode"/>
    [JsonPropertyName("dev")]
    public bool? DevMode { get; set; }

    /// <inheritdoc cref="IAppConfig.DebugMode"/>
    [JsonPropertyName("debug")]
    public bool? DebugMode { get; set; }

    /// <inheritdoc cref="IAppConfig.DefaultPasswordLength"/>
    [JsonPropertyName("default_password_length")]
    public int? DefaultPasswordLength { get; set; }
}