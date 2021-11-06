using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using PassMeta.DesktopApp.Common.Interfaces.Services;
using PassMeta.DesktopApp.Core.Services;
using PassMeta.DesktopApp.Core.Utils;
using PassMeta.DesktopApp.Ui.Services;
using PassMeta.DesktopApp.Ui.ViewModels.Main;
using PassMeta.DesktopApp.Ui.Views.Main;
using Splat;

namespace PassMeta.DesktopApp.Ui
{
    using Avalonia.Controls.Notifications;
    using Avalonia.Layout;

    public class App : Application
    {
        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public override void OnFrameworkInitializationCompleted()
        {
            BeforeLaunch();
            
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
            
            var desktop = (IClassicDesktopStyleApplicationLifetime)Current.ApplicationLifetime;
            desktop.MainWindow.Close(true);
            desktop.MainWindow = window;
        }

        private static void BeforeLaunch()
        {
            var logger = new LogService();
            
            Locator.CurrentMutable.RegisterConstant<ILogService>(logger);
            
            Locator.CurrentMutable.RegisterConstant<IDialogService>(new DialogService(logger));
            
            AppConfig.LoadAndSetCurrentAsync().GetAwaiter().GetResult();

            Locator.CurrentMutable.RegisterConstant<IOkBadService>(new OkBadService());

            Locator.CurrentMutable.RegisterConstant<IAuthService>(new AuthService());

            Locator.CurrentMutable.RegisterConstant<IAccountService>(new AccountService());
            
            Locator.CurrentMutable.RegisterConstant<IPassFileService>(new PassFileService());
            
            Locator.CurrentMutable.RegisterConstant<ICryptoService>(new CryptoService());
        }

        private static MainWindow MakeWindow()
        {
            var win = new MainWindow { DataContext = new MainWindowViewModel() };
            
            Locator.CurrentMutable.UnregisterCurrent<INotificationManager>();
            Locator.CurrentMutable.RegisterConstant<INotificationManager>(new WindowNotificationManager(win)
            {
                HorizontalAlignment = HorizontalAlignment.Right,
                VerticalAlignment = VerticalAlignment.Bottom
            });

            return win;
        }
    }
}