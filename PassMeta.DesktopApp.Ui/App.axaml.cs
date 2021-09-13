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
                desktop.MainWindow = new MainWindow
                {
                    DataContext = new MainWindowViewModel(),
                };
            }

            base.OnFrameworkInitializationCompleted();
        }

        public static void Restart()
        {
            var window = new MainWindow
            {
                DataContext = new MainWindowViewModel(),
            };
            window.Show();
            
            var desktop = (IClassicDesktopStyleApplicationLifetime)Current.ApplicationLifetime;
            desktop.MainWindow.Close(true);
            desktop.MainWindow = window;
        }

        private void BeforeLaunch()
        {
            Locator.CurrentMutable.RegisterConstant<IDialogService>(new DialogService());
            
            AppConfig.LoadAndSetCurrentAsync().GetAwaiter().GetResult();

            Locator.CurrentMutable.RegisterConstant<IOkBadService>(new OkBadService());

            Locator.CurrentMutable.RegisterConstant<IAuthService>(new AuthService());

            Locator.CurrentMutable.RegisterConstant<IAccountService>(new AccountService());
            
            Locator.CurrentMutable.RegisterConstant<IPassFileService>(new PassFileService());
            
            Locator.CurrentMutable.RegisterConstant<ICryptoService>(new CryptoService());
        }
    }
}