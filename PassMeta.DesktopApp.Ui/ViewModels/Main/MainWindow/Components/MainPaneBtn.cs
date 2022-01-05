namespace PassMeta.DesktopApp.Ui.ViewModels.Main.MainWindow.Components
{
    using System;
    using System.Reactive.Linq;
    using ReactiveUI;

    public class MainPaneBtn : ReactiveObject
    {
        private bool _isActive;
        public bool IsActive
        {
            get => _isActive;
            set => this.RaiseAndSetIfChanged(ref _isActive, value);
        }

        private readonly ObservableAsPropertyHelper<string> _content;
        public string Content => _content.Value;

        public MainPaneBtn(string text, string icon, IObservable<bool> shortModeObservable)
        {
            _content = shortModeObservable.Select(isShort => isShort
                ? icon
                : text).ToProperty(this, nameof(Content));
        }
    }
}