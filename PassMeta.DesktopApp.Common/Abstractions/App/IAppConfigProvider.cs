using System;

namespace PassMeta.DesktopApp.Common.Abstractions.App;

/// <summary>
/// <see cref="IAppConfig"/> provider.
/// </summary>
/// <remarks>Singleton.</remarks>
public interface IAppConfigProvider
{
    /// <summary>
    /// Current application context.
    /// </summary>
    IAppConfig Current { get; }

    /// <summary>
    /// Represents <see cref="Current"/>.
    /// </summary>
    IObservable<IAppConfig> CurrentObservable { get; }
}