namespace PassMeta.DesktopApp.Ui.Utils
{
    using System;
    using Interfaces;

    /// <summary>
    /// Preloader manager.
    /// </summary>
    public sealed class Preloader : IDisposable
    {
        private readonly IPreloaderSupport _vm;
        private bool _disposed;
        
        /// <summary></summary>
        public Preloader(IPreloaderSupport viewModel)
        {
            _vm = viewModel;
        }
        
        /// <summary>
        /// Show preloader.
        /// </summary>
        /// <returns>this.</returns>
        /// <exception cref="Exception">If object is disposed.</exception>
        public Preloader Start()
        {
            if (_disposed)
                throw new Exception("Preloader manager object is disposed, it can't be started");
            
            _vm.PreloaderEnabled = true;
            return this;
        }

        /// <summary>
        /// Hide preloader, if object is not disposed.
        /// </summary>
        public void Finish()
        {
            if (!_disposed)
                _vm.PreloaderEnabled = false;
        }
        
        /// <inheritdoc />
        public void Dispose()
        {
            Finish();
            _disposed = true;
        }
    }
}