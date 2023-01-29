namespace PassMeta.DesktopApp.Common.Abstractions.AppContext;

/// <summary>
/// <see cref="IUserContext"/> provider.
/// </summary>
/// <remarks>Singleton.</remarks>
public interface IUserContextProvider
{
    /// <summary>
    /// Current user context.
    /// </summary>
    IUserContext Current { get; }
}