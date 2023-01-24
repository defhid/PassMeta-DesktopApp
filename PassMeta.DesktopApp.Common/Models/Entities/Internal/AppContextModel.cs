using System.Collections.Generic;
using System.Linq;
using System.Net;
using PassMeta.DesktopApp.Common.Abstractions.AppContext;
using PassMeta.DesktopApp.Common.Models.Dto.Internal;

namespace PassMeta.DesktopApp.Common.Models.Entities.Internal;

/// <inheritdoc />
public class AppContextModel : IAppContext
{
    /// <inheritdoc />
    public IReadOnlyList<Cookie> Cookies { get; set; }

    /// <inheritdoc />
    public User? User { get; set; }

    /// <inheritdoc />
    public string? ServerId { get; set; }

    /// <inheritdoc />
    public string? ServerVersion { get; set; }

    /// <summary></summary>
    public AppContextModel(AppContextDto dto)
    {
        Cookies = dto.Cookies ?? new List<Cookie>();
        User = dto.User;
        ServerId = dto.ServerId;
        ServerVersion = dto.ServerVersion;
    }

    /// <summary></summary>
    public AppContextDto ToDto() => new()
    {
        User = User,
        Cookies = Cookies.ToList(),
        ServerId = ServerId,
        ServerVersion = ServerVersion
    };

    /// <summary>
    /// Get a new model with copied properties.
    /// </summary>
    public AppContextModel Copy() => (AppContextModel) MemberwiseClone();
}