using System.Collections.Generic;
using System.Linq;
using System.Net;
using PassMeta.DesktopApp.Common.Abstractions;
using PassMeta.DesktopApp.Common.Models.Entities;
using PassMeta.DesktopApp.Common.Models.Settings;

namespace PassMeta.DesktopApp.Core.Models;

/// <inheritdoc />
public class AppContextModel : IAppContext
{
    /// <inheritdoc />
    public IReadOnlyList<Cookie> Cookies { get; set; }

    /// <inheritdoc />
    public User? User { get; set; }

    /// <inheritdoc />
    public uint PassFilesCounter { get; set; }

    /// <inheritdoc />
    public string? ServerId { get; set; }

    /// <inheritdoc />
    public string? ServerVersion { get; set; }

    /// <summary></summary>
    public AppContextModel(AppContextDto dto)
    {
        Cookies = dto.Cookies ?? new List<Cookie>();
        User = dto.User;
        PassFilesCounter = dto.PassFilesCounter ?? 0;
        ServerId = dto.ServerId;
    }

    /// <summary></summary>
    public AppContextDto ToDto() => new()
    {
        User = User,
        Cookies = Cookies.ToList(),
        PassFilesCounter = PassFilesCounter,
        ServerId = ServerId
    };

    /// <summary>
    /// Get a new model with copied properties.
    /// </summary>
    public AppContextModel Copy() => (AppContextModel) MemberwiseClone();
}