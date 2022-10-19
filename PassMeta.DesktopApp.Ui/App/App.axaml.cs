namespace PassMeta.DesktopApp.Ui.App
{
    using DesktopApp.Core.Services;
    using DesktopApp.Core.Utils;
    using DesktopApp.Ui.ViewModels.Main.MainWindow;
    using DesktopApp.Ui.Views.Main;
    
    using Avalonia;
    using Avalonia.Controls.ApplicationLifetimes;
    using Avalonia.Controls.Notifications;
    using Avalonia.Layout;
    using Avalonia.Markup.Xaml;

    using System.Reflection;
    using System.Threading;
    using System.Threading.Tasks;
    using Common.Abstractions.Services;
    using Common.Abstractions.Services.PassFile;
    using Common.Enums;
    using Core;
    using Interfaces.UiServices;
    using ReactiveUI;
    using Services;
    using Splat;

    public class App : Application
    {
        public override void Initialize()
        {
            Task.Run(BeforeLaunchAsync).GetAwaiter().GetResult();
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

        private static async Task BeforeLaunchAsync()
        {
            EnvironmentContainer.Initialize(Locator.Current);

            RegisterBaseServices();
            RegisterUiServices();

            await StartUp.LoadConfigurationAsync();

            _ = AppConfig.CurrentObservable.Subscribe(new AppConfigObserver());
            _ = AppContext.CurrentObservable.Subscribe(new AppContextObserver());

            _ = Task.Run(StartUp.CheckSystemAsync);
        }

        private static void RegisterBaseServices()
        {
            Locator.CurrentMutable.RegisterConstant<ILogService>(new LogService());
            
            Locator.CurrentMutable.RegisterConstant<IDialogService>(new DialogService());

            Locator.CurrentMutable.RegisterConstant<IOkBadService>(new OkBadService());

            Locator.CurrentMutable.RegisterConstant<IAuthService>(new AuthService());

            Locator.CurrentMutable.RegisterConstant<IAccountService>(new AccountService());
            
            Locator.CurrentMutable.RegisterConstant<ICryptoService>(new CryptoService());
            
            Locator.CurrentMutable.RegisterConstant<IPassFileRemoteService>(new PassFileRemoteService());
            
            Locator.CurrentMutable.RegisterConstant<IPassFileSyncService>(new PassFileSyncService());
            
            Locator.CurrentMutable.RegisterConstant<IPassFileImportService>(new PassFilePwdImportService(), PassFileType.Pwd.ToString());

            Locator.CurrentMutable.RegisterConstant<IPassFileExportService>(new PassFilePwdExportService(), PassFileType.Pwd.ToString());
            
            Locator.CurrentMutable.RegisterConstant<IPwdMergePreparingService>(new PwdMergePreparingService());
            
            Locator.CurrentMutable.RegisterConstant<IClipboardService>(new ClipboardService());
        }

        private static void RegisterUiServices()
        {
            Locator.CurrentMutable.RegisterConstant<IPassFileExportUiService>(new PassFileExportUiService());
            
            Locator.CurrentMutable.RegisterConstant<IPassFileMergeUiService>(new PassFileMergeUiService());
            
            Locator.CurrentMutable.RegisterConstant<IPassFileRestoreUiService>(new PassFileRestoreUiService());
            
            Locator.CurrentMutable.RegisterViewsForViewModels(Assembly.GetExecutingAssembly());
        }

        private static MainWindow MakeWindow()
        {
            Thread.CurrentThread.CurrentCulture = Common.Resources.Culture;
            Thread.CurrentThread.CurrentUICulture = Common.Resources.Culture;

            var dataContext = new MainWindowViewModel();

            var win = new MainWindow { DataContext = dataContext };

            win.Closed += (_, _) => dataContext.Dispose();
            
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