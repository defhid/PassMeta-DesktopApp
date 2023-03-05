using System.Reactive.Linq;
using Avalonia.Media;
using PassMeta.DesktopApp.Common;
using PassMeta.DesktopApp.Common.Abstractions.Utils.PassMetaClient;
using PassMeta.DesktopApp.Common.Extensions;
using ReactiveUI;
using Splat;

namespace PassMeta.DesktopApp.Ui.Models.ViewModels.Windows.MainWin.Extra;

public class AppMode : ReactiveObject
{
    private readonly ObservableAsPropertyHelper<string> _text;
    public string Text => _text.Value;

    private readonly ObservableAsPropertyHelper<ISolidColorBrush> _foreground;
    public ISolidColorBrush Foreground => _foreground.Value;

    public AppMode()
    {
        var passMetaClient = Locator.Current.Resolve<IPassMetaClient>();

        _text = passMetaClient.OnlineObservable.Select(online => online 
                ? Resources.APP__ONLINE_MODE
                : Resources.APP__OFFLINE_MODE)
            .ToProperty(this, nameof(Text));

        _foreground = passMetaClient.OnlineObservable.Select(online => online 
                ? Brushes.Green 
                : Brushes.SlateGray)
            .ToProperty(this, nameof(Foreground));
    }
}