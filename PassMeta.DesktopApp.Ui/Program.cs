using Avalonia;
using Avalonia.Logging;
using Avalonia.ReactiveUI;
using PassMeta.DesktopApp.Ui.App;

namespace PassMeta.DesktopApp.Ui;

public static class Program
{
    public static void Main(string[] args)
    {
#if DEBUG
        Logger.Sink = new AppLogSink();
#else
        Logger.Sink = new TraceLogSink(LogEventLevel.Warning);
#endif

        AppBuilder.Configure<App.App>()
            .UsePlatformDetect()
            .UseReactiveUI()
            .StartWithClassicDesktopLifetime(args);
    }
}