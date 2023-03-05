using Avalonia;
using Avalonia.Logging;
using Avalonia.ReactiveUI;
using PassMeta.DesktopApp.Ui.App;

namespace PassMeta.DesktopApp.Ui;

public static class Program
{
    public static AppBuilder BuildAvaloniaApp() 
        => AppBuilder.Configure<App.App>().UsePlatformDetect()
            .UseReactiveUI();

    public static void Main(string[] args)
    {
        Logger.Sink = new AppLogSink(LogEventLevel.Error);

        BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
    }
}