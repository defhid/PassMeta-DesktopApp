namespace PassMeta.DesktopApp.Ui
{
    using Avalonia;
    using Avalonia.ReactiveUI;

    public static class Program
    {
        public static void Main(string[] args) => AppBuilder.Configure<App.App>()
            .UsePlatformDetect()
            .LogToTrace()
            .UseReactiveUI()
            .StartWithClassicDesktopLifetime(args);
    }
}