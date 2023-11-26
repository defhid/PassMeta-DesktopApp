using System.Collections.Generic;
using System.Net;
using System.Text.Json.Serialization;
using PassMeta.DesktopApp.Common.Abstractions.App;
using PassMeta.DesktopApp.Common.Models.Entities;

namespace PassMeta.DesktopApp.Common.Models.Dto.Internal;

/// <summary>
/// Application context DTO.
/// </summary>
public class AppContextDto
{
    /// <inheritdoc cref="IAppContext.User"/>
    [JsonPropertyName("user")]
    public User? User { get; set; }

    /// <inheritdoc cref="IAppContext.Cookies"/>
    [JsonPropertyName("cookies")]
    public List<Cookie>? Cookies { get; set; }

    /// <inheritdoc cref="IAppContext.ServerId"/>
    [JsonPropertyName("sid")]
    public string? ServerId { get; set; }

    /// <inheritdoc cref="IAppContext.ServerVersion"/>
    [JsonPropertyName("sver")]
    public string? ServerVersion { get; set; }
}