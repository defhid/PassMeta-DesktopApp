namespace PassMeta.DesktopApp.Ui
{
    using System.Reflection;
    using Avalonia;
    using Avalonia.ReactiveUI;
    using ReactiveUI;
    using Splat;
    
    class Program
    {
        public static void Main(string[] args)
        {
            Locator.CurrentMutable.RegisterViewsForViewModels(Assembly.GetExecutingAssembly());

            AppBuilder.Configure<App>()
                .UsePlatformDetect()
                .LogToTrace()
                .UseReactiveUI()
                .StartWithClassicDesktopLifetime(args);
        }
    }
}