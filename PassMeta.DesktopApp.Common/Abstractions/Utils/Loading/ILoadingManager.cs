namespace PassMeta.DesktopApp.Common.Abstractions.Utils.Loading
{
    using System;

    /// <summary>
    /// Loading manager.
    /// </summary>
    public interface ILoadingManager : ILoadingState
    {
        /// <summary>
        /// Begin loading.
        /// </summary>
        /// <returns>Loading finisher.</returns>
        /// <remarks>
        /// If method is called multiple times, <see cref="ILoadingState.Current"/> value
        /// will be FALSE only when all the finishers are disposed.</remarks>
        IDisposable Begin();
    }
}