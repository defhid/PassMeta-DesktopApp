namespace PassMeta.DesktopApp.Ui.ViewModels.Storage.Storage.Components
{
    using System;
    using System.Reactive.Linq;
    using ReactiveUI;

    public class PassFileBarExpander : ReactiveObject
    {
        private bool _isOpened;
        public bool IsOpened
        {
            get => _isOpened;
            set => this.RaiseAndSetIfChanged(ref _isOpened, value);
        }
        
        public readonly IObservable<bool> IsOpenedObservable;
        
        public readonly IObservable<bool> ShortModeObservable;

        public bool AutoExpanding = true;

        private bool _autoExpandingFreeze;

        public PassFileBarExpander()
        {
            IsOpenedObservable = this.WhenAnyValue(vm => vm.IsOpened);
            ShortModeObservable = IsOpenedObservable.Select(isOpened => !isOpened);
        }

        public void TryExecuteAutoExpanding(bool isOpened)
        {
            if (AutoExpanding && !_autoExpandingFreeze)
                IsOpened = isOpened;
        }

        public DisabledAutoExpanding DisableAutoExpandingScoped() => new DisabledAutoExpanding(this).Start();
        
        public sealed class DisabledAutoExpanding : IDisposable
        {
            private readonly PassFileBarExpander _expander;
            private bool _disposed;

            public DisabledAutoExpanding(PassFileBarExpander expander)
            {
                _expander = expander;
            }

            public DisabledAutoExpanding Start()
            {
                if (_disposed)
                    throw new Exception("Starting disposed " + nameof(DisabledAutoExpanding));
                
                _expander._autoExpandingFreeze = true;
                return this;
            }

            public void Stop() => Dispose();

            public void Dispose()
            {
                if (_disposed) return;
                _expander._autoExpandingFreeze = false;
                _disposed = true;
            }
        }
    }
}