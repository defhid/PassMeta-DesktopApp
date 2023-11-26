using Avalonia;
using Avalonia.Logging;
using Avalonia.ReactiveUI;
using PassMeta.DesktopApp.Ui.App;

namespace PassMeta.DesktopApp.Ui;

public static class Program
{
    // Used by previews!
    // ReSharper disable once MemberCanBePrivate.Global
    public static AppBuilder BuildAvaloniaApp() => AppBuilder.Configure<App.App>()
        .UsePlatformDetect()
        .UseReactiveUI()
        .WithInterFont();

    public static void Main(string[] args)
    {
        using var appLogSink = new AppLogSink(LogEventLevel.Error);
        Logger.Sink = appLogSink;

        BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
    }
}