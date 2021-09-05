using System.Reflection;
using Avalonia;
using Avalonia.ReactiveUI;
using ReactiveUI;
using Splat;

namespace PassMeta.DesktopApp.Ui
{
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