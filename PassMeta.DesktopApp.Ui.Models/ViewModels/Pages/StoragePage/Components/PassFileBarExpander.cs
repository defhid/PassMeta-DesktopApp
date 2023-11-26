using System;
using ReactiveUI;

namespace PassMeta.DesktopApp.Ui.Models.ViewModels.Pages.StoragePage.Components;

public class PassFileBarExpander : ReactiveObject
{
    private bool _isOpened;
    public bool IsOpened
    {
        get => _isOpened;
        set => this.RaiseAndSetIfChanged(ref _isOpened, value);
    }
        
    public readonly IObservable<bool> IsOpenedObservable;

    public bool AutoExpanding = true;

    private bool _autoExpandingFreeze;

    public PassFileBarExpander()
    {
        IsOpenedObservable = this.WhenAnyValue(vm => vm.IsOpened);
    }

    public void TryExecuteAutoExpanding(bool isOpened)
    {
        if (AutoExpanding && !_autoExpandingFreeze)
            IsOpened = isOpened;
    }

    public IDisposable DisableAutoExpandingScoped() => new DisabledAutoExpanding(this).Start();
        
    private sealed class DisabledAutoExpanding : IDisposable
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

        public void Dispose()
        {
            if (_disposed) return;
            _expander._autoExpandingFreeze = false;
            _disposed = true;
        }
    }
}