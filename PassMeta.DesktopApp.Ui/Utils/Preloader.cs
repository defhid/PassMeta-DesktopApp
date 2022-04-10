namespace PassMeta.DesktopApp.Ui.Utils
{
    using System;
    using System.Collections.Generic;
    using Interfaces;

    /// <summary>
    /// Preloader manager.
    /// </summary>
    public sealed class Preloader : IDisposable
    {
        private static readonly Dictionary<IPreloaderSupport, int> Counters = new();

        private readonly IPreloaderSupport _vm;
        private bool _disposed;
        
        /// <summary></summary>
        public Preloader(IPreloaderSupport viewModel)
        {
            _vm = viewModel;
            lock (Counters)
            {
                if (!Counters.ContainsKey(_vm))
                {
                    Counters[_vm] = 0;
                }
            }
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

            lock (Counters)
            {
                ++Counters[_vm];
                _vm.PreloaderEnabled = true;
            }

            return this;
        }

        /// <summary>
        /// Hide preloader, if object is not disposed.
        /// </summary>
        public void Finish()
        {
            if (_disposed) return;
            lock (Counters)
            {
                if (--Counters[_vm] == 0)
                {
                    _vm.PreloaderEnabled = false;
                }
            }
        }
        
        /// <inheritdoc />
        public void Dispose()
        {
            Finish();
            _disposed = true;
        }
    }
}