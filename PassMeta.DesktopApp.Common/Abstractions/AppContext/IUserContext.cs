namespace PassMeta.DesktopApp.Common.Abstractions.AppContext;

/// <summary>
/// Application user context.
/// </summary>
public interface IUserContext
{
    /// <summary>
    /// User id.
    /// </summary>
    int? UserId { get; }
    
    /// <summary>
    /// User server id.
    /// </summary>
    string? UserServerId { get; }
    
    /// <summary>
    /// An unique id that combines <see cref="UserId"/> and <see cref="UserServerId"/>.
    /// </summary>
    string UniqueId { get; }
}