using System;

namespace PassMeta.DesktopApp.Common.Abstractions.UserContext;

/// <summary>
/// <see cref="IUserContext"/> provider.
/// </summary>
/// <remarks>Singleton.</remarks>
public interface IUserContextProvider
{
    /// <summary>
    /// Current application context.
    /// </summary>
    IUserContext Current { get; }

    /// <summary>
    /// Represents <see cref="Current"/>.
    /// </summary>
    IObservable<IUserContext> CurrentObservable { get; }
}