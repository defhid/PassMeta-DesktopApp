using System;

namespace PassMeta.DesktopApp.Common.Abstractions.AppContext;

/// <summary>
/// <see cref="IAppContext"/> provider.
/// </summary>
/// <remarks>Singleton.</remarks>
public interface IAppContextProvider
{
    /// <summary>
    /// Current application context.
    /// </summary>
    IAppContext Current { get; }

    /// <summary>
    /// Represents <see cref="Current"/>.
    /// </summary>
    IObservable<IAppContext> CurrentObservable { get; }
}