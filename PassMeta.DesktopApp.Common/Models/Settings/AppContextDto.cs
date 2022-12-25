using System.Collections.Generic;
using System.Net;
using Newtonsoft.Json;

using PassMeta.DesktopApp.Common.Abstractions;
using PassMeta.DesktopApp.Common.Models.Entities;

namespace PassMeta.DesktopApp.Common.Models.Settings;

/// <summary>
/// Application context DTO.
/// </summary>
public class AppContextDto
{
    /// <inheritdoc cref="IAppContext.User"/>
    [JsonProperty("user")]
    public User? User { get; set; }

    /// <inheritdoc cref="IAppContext.Cookies"/>
    [JsonProperty("cookies")]
    public List<Cookie>? Cookies { get; set; }

    /// <inheritdoc cref="IAppContext.PassFilesCounter"/>
    [JsonProperty("pfcnt")]
    public uint? PassFilesCounter { get; set; }

    /// <inheritdoc cref="IAppContext.ServerId"/>
    [JsonProperty("sid")]
    public string? ServerId { get; set; }
}