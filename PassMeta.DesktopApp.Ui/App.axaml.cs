using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using PassMeta.DesktopApp.Common.Interfaces.Services;
using PassMeta.DesktopApp.Common.Models.Entities.Response;
using PassMeta.DesktopApp.Core.Services;
using PassMeta.DesktopApp.Core.Utils;
using PassMeta.DesktopApp.Ui.Services;
using PassMeta.DesktopApp.Ui.ViewModels;
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

        private static void BeforeLaunch()
        {
            Locator.CurrentMutable.RegisterConstant<IDialogService>(new DialogService());
            
            AppConfig.Load();

            PassMetaInfo? info = null;

            if (AppConfig.Current.IsServerUrlCorrect)
            {
                info = PassMetaApi.GetAsync<PassMetaInfo>("/info", true)
                    .GetAwaiter().GetResult()?.Data;
            }
            
            if (info is not null)
                AppConfig.LoadPassMetaInfo(info);
            
            Locator.CurrentMutable.RegisterConstant<IOkBadService>(new OkBadService(info?.OkBadMessagesTranslatePack));

            Locator.CurrentMutable.RegisterConstant<IAuthService>(new AuthService());

            Locator.CurrentMutable.RegisterConstant<IAccountService>(new AccountService());
            
            Locator.CurrentMutable.RegisterConstant<IPassFileService>(new PassFileService());
        }
    }
}