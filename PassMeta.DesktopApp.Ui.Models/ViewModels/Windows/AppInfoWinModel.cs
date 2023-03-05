using PassMeta.DesktopApp.Common.Extensions;
using PassMeta.DesktopApp.Common.Models.App;
using ReactiveUI;
using Splat;

namespace PassMeta.DesktopApp.Ui.Models.ViewModels.Windows;

public class AppInfoWinModel : ReactiveObject
{
    public string Version => $"{_appInfo.Version} ({_appInfo.Bit})";

    public string Copyright => $"{_appInfo.Copyright} {_appInfo.Author}";

    public string PassMetaIcon => "Assets/AvaRes/PassMeta.ico";

    private readonly AppInfo _appInfo = Locator.Current.Resolve<AppInfo>();
}