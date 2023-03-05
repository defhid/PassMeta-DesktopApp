using System.Diagnostics;
using PassMeta.DesktopApp.Common.Abstractions.App;

namespace PassMeta.DesktopApp.Common.Models.Entities.Internal;

/// <inheritdoc />
public class UserContextModel : IUserContext
{
    /// <summary></summary>
    public UserContextModel(int? userId, string? userServerId)
    {
        Debug.Assert(userId is null || !string.IsNullOrWhiteSpace(userServerId));
        
        UserId = userId;
        UserServerId = userServerId;
        UniqueId = $"{userId ?? 0}__{UserServerId}";
    }

    /// <inheritdoc />
    public int? UserId { get; }
    
    /// <inheritdoc />
    public string? UserServerId { get; }

    /// <inheritdoc />
    public string UniqueId { get; }
}