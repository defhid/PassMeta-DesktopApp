namespace PassMeta.DesktopApp.Ui.App
{
    using DesktopApp.Common.Interfaces.Services;
    using DesktopApp.Core.Services;
    using DesktopApp.Core.Utils;
    using DesktopApp.Ui.ViewModels.Main.MainWindow;
    using DesktopApp.Ui.Views.Main;
    
    using Avalonia;
    using Avalonia.Controls.ApplicationLifetimes;
    using Avalonia.Controls.Notifications;
    using Avalonia.Layout;
    using Avalonia.Markup.Xaml;
    
    using System.Threading.Tasks;
    using System.Reflection;
    using System.Threading;
    using Common.Interfaces.Services.PassFile;
    using Core;
    using ReactiveUI;
    using Services;
    using Splat;

    public class App : Application
    {
        public override void Initialize()
        {
            Task.Run(BeforeLaunch).GetAwaiter().GetResult();
            AvaloniaXamlLoader.Load(this);
        }

        public override void OnFrameworkInitializationCompleted()
        {
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                desktop.MainWindow = MakeWindow();
            }

            base.OnFrameworkInitializationCompleted();
        }

        public static void Restart()
        {
            var window = MakeWindow();
            window.Show();
            
            var desktop = (IClassicDesktopStyleApplicationLifetime)Current!.ApplicationLifetime!;
            desktop.MainWindow.Close(true);
            desktop.MainWindow = window;
        }

        private static async Task BeforeLaunch()
        {
            EnvironmentContainer.Initialize(Locator.Current);

            var logger = new LogService();

            Locator.CurrentMutable.RegisterConstant<ILogService>(logger);
            
            Locator.CurrentMutable.RegisterConstant<IDialogService>(new DialogService());

            await StartUp.CheckSystemAndLoadApplicationConfigAsync();

            AppConfig.OnCultureChanged += Restart;

            Locator.CurrentMutable.RegisterConstant<IOkBadService>(new OkBadService());

            Locator.CurrentMutable.RegisterConstant<IAuthService>(new AuthService());

            Locator.CurrentMutable.RegisterConstant<IAccountService>(new AccountService());
            
            Locator.CurrentMutable.RegisterConstant<ICryptoService>(new CryptoService());
            
            Locator.CurrentMutable.RegisterConstant<IPassFileService>(new PassFileService());
            
            Locator.CurrentMutable.RegisterConstant<IPassFileImportService>(new PassFileImportService());
            
            Locator.CurrentMutable.RegisterConstant<IPassFileExportService>(new PassFileExportService());
            
            Locator.CurrentMutable.RegisterConstant<IClipboardService>(new ClipboardService());
            
            Locator.CurrentMutable.RegisterViewsForViewModels(Assembly.GetExecutingAssembly());
            
            Locator.Current.GetService<ILogService>()!.OptimizeLogs();
        }

        private static MainWindow MakeWindow()
        {
            Thread.CurrentThread.CurrentCulture = Common.Resources.Culture;
            Thread.CurrentThread.CurrentUICulture = Common.Resources.Culture;
            
            var win = new MainWindow { DataContext = new MainWindowViewModel() };
            
            Locator.CurrentMutable.UnregisterCurrent<INotificationManager>();
            Locator.CurrentMutable.RegisterConstant<INotificationManager>(new WindowNotificationManager(win)
            {
                HorizontalAlignment = HorizontalAlignment.Right,
                VerticalAlignment = VerticalAlignment.Bottom,
                Opacity = 0.8,
                Margin = Thickness.Parse("0 0 0 -4")
            });

            return win;
        }
    }
}