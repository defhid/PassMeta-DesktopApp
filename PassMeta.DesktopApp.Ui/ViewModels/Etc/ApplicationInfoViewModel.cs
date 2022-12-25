namespace PassMeta.DesktopApp.Ui.ViewModels.Etc
{
    using Core;
    using ReactiveUI;
        
    using ReactCommand = ReactiveUI.ReactiveCommand<System.Reactive.Unit, System.Reactive.Unit>;

    public class ApplicationInfoViewModel : ReactiveObject
    {
        public static string Version => $"{AppInfo.Version} ({AppInfo.Bit})";

        public static string Copyright => $"{AppInfo.Copyright} {AppInfo.Author}";

        public static string PassMetaIcon => "Assets/AvaRes/PassMeta.ico";
    }
}