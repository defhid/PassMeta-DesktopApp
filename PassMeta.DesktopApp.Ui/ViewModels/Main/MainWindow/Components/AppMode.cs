namespace PassMeta.DesktopApp.Ui.ViewModels.Main.MainWindow.Components
{
    using System.Reactive.Linq;
    using Avalonia.Media;
    using Common;
    using Core.Utils;
    using ReactiveUI;

    public class AppMode : ReactiveObject
    {
        private readonly ObservableAsPropertyHelper<string> _text;
        public string Text => _text.Value;

        private readonly ObservableAsPropertyHelper<ISolidColorBrush> _foreground;
        public ISolidColorBrush Foreground => _foreground.Value;

        public AppMode()
        {
            _text = PassMetaClient.OnlineObservable.Select(online => online 
                    ? Resources.APP__ONLINE_MODE
                    : Resources.APP__OFFLINE_MODE)
                .ToProperty(this, nameof(Text));

            _foreground = PassMetaClient.OnlineObservable.Select(online => online 
                    ? Brushes.Green 
                    : Brushes.SlateGray)
                .ToProperty(this, nameof(Foreground));
        }
    }
}