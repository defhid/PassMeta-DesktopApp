namespace PassMeta.DesktopApp.Ui.ViewModels.Components
{
    using System;
    using System.Reactive;
    using System.Reactive.Linq;
    using System.Windows.Input;
    using Avalonia;
    using ReactiveUI;

    public class BtnState : ReactiveObject
    {
        private readonly ObservableAsPropertyHelper<string>? _content;
        public string Content => _content!.Value;
        
        private readonly ObservableAsPropertyHelper<ReactiveCommand<Unit, Unit>>? _command;
        public ICommand Command => _command!.Value;
        
        private readonly ObservableAsPropertyHelper<bool>? _isVisible;
        public bool IsVisible => _isVisible!.Value;

        private readonly ObservableAsPropertyHelper<double>? _opacity;
        public double Opacity => _opacity!.Value;
        
        private readonly ObservableAsPropertyHelper<Thickness>? _padding;
        public Thickness Padding => _padding!.Value;

        public BtnState(IObservable<string>? contentObservable = null,
            IObservable<ReactiveCommand<Unit, Unit>>? commandObservable = null,
            IObservable<bool>? isVisibleObservable = null,
            IObservable<double>? opacityObservable = null,
            IObservable<Thickness>? paddingObservable = null)
        {
            _content = contentObservable?.ToProperty(this, nameof(Content));
            _command = commandObservable?.ToProperty(this, nameof(Command));
            
            _isVisible = (isVisibleObservable ?? Observable.Return(true))
                .ToProperty(this, nameof(IsVisible));
            
            _opacity = (opacityObservable ?? Observable.Return(1d))
                .ToProperty(this, nameof(Opacity));
            
            _padding = paddingObservable?.ToProperty(this, nameof(Padding));
        }
    }
}