namespace PassMeta.DesktopApp.Common.Abstractions.Utils.Loading;

using System;

/// <summary>
/// Loading state.
/// </summary>
public interface ILoadingState
{
    /// <summary>
    /// Loading is happening now.
    /// </summary>
    bool Active { get; }

    /// <summary>
    /// Represents <see cref="Active"/>.
    /// </summary>
    IObservable<bool> ActiveObservable { get; }
}