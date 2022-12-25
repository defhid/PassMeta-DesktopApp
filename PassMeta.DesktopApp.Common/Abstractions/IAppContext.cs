using System.Collections.Generic;
using System.Net;
using PassMeta.DesktopApp.Common.Models.Entities;

namespace PassMeta.DesktopApp.Common.Abstractions;

/// <summary>
/// Application context: cookies, user, etc.
/// </summary>
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
    /// Total count of locally created passfiles.
    /// </summary>
    public uint PassFilesCounter { get; }

    /// <summary>
    /// Server identifier.
    /// </summary>
    public string? ServerId { get; }

    /// <summary>
    /// Server version. If not null, indicates correct <see cref="IAppConfig.ServerUrl"/>
    /// and internet connection has been established at least once.
    /// </summary>
    public string? ServerVersion { get; }
}