namespace PassMeta.DesktopApp.Common.Abstractions.Utils.Loading
{
    using System;

    /// <summary>
    /// Loading state.
    /// </summary>
    public interface ILoadingState
    {
        /// <summary>
        /// Loading is happening now.
        /// </summary>
        bool Current { get; }
        
        /// <summary>
        /// Represents <see cref="Current"/>.
        /// </summary>
        IObservable<bool> CurrentObservable { get; }
    }
}