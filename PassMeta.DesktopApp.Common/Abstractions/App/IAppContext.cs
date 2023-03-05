using System.Collections.Generic;
using System.Net;
using PassMeta.DesktopApp.Common.Models.Entities;

namespace PassMeta.DesktopApp.Common.Abstractions.App;

/// <summary>
/// Application context: cookies, user, etc.
/// </summary>
/// <remarks>Scoped.</remarks>
public interface IAppContext
{
    /// <summary>
    /// Cookies from the server.
    /// </summary>
    public IReadOnlyList<Cookie> Cookies { get; }

    /// <summary>
    /// Application user.
    /// </summary>
    public User? User { get; }

    /// <summary>
    /// Server identifier.
    /// </summary>
    public string? ServerId { get; }

    /// <summary>
    /// Server version from last connection.
    /// </summary>
    public string? ServerVersion { get; }
}