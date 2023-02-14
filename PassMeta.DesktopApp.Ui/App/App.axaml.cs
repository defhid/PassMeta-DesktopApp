using System.Net;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Controls.Notifications;
using Avalonia.Layout;
using Avalonia.Markup.Xaml;

using Splat;
using System.Threading.Tasks;
using PassMeta.DesktopApp.Common.Abstractions.AppConfig;
using PassMeta.DesktopApp.Common.Abstractions.Services;
using PassMeta.DesktopApp.Common.Abstractions.Utils.Logging;
using PassMeta.DesktopApp.Common.Abstractions.Utils.PassMetaClient;
using PassMeta.DesktopApp.Core.Extensions;
using PassMeta.DesktopApp.Core.Utils;
using PassMeta.DesktopApp.Ui.App.Observers;
using PassMeta.DesktopApp.Ui.ViewModels.Main.MainWindow;
using PassMeta.DesktopApp.Ui.Views.Main;

namespace PassMeta.DesktopApp.Ui.App;

public class App : Application
{
    private static MainWindow? _mainWindow;
    public static MainWindow? MainWindow
    {
        get => _mainWindow;
        private set
        {
            var desktop = (IClassicDesktopStyleApplicationLifetime)Current!.ApplicationLifetime!;
            desktop.MainWindow = value;
            _mainWindow = value;
        }
    }

    public override void Initialize()
    {
        Task.Run(BeforeLaunchAsync).GetAwaiter().GetResult();
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        MainWindow = MakeWindow();
        Locator.Current.Resolve<IDialogService>().Flush();
    }

    public static void ReopenMainWindow()
    {
        var window = MakeWindow();
        window.Show();

        MainWindow?.Close();
        MainWindow = window;

        Locator.Current.Resolve<IDialogService>().Flush();
    }

    private static async Task BeforeLaunchAsync()
    {
        using var loading = AppLoading.General.Begin();

        ServicePointManager.ServerCertificateValidationCallback = (_, _, _, _) => true;

        DependencyInstaller.RegisterServices();
        DependencyInstaller.RegisterViewsForViewModels();

        var logManager = Locator.Current.Resolve<ILogsManager>();
        var dialogService = Locator.Current.Resolve<IDialogService>();

        logManager.InternalErrorOccured += (_, ev) => 
            dialogService.ShowError(ev.Message, more: ev.Exception.ToString());

        await StartUp.LoadAsync();

        Locator.Current.Resolve<IAppConfigManager>()
            .CurrentObservable.Subscribe(new AppConfigObserver());

        Locator.Current.Resolve<IPassMetaClient>()
            .OnlineObservable.Subscribe(new OnlineObserver());

        _ = Task.Run(StartUp.CleanUp);
    }

    private static MainWindow MakeWindow()
    {
        var win = new MainWindow { DataContext = new MainWindowViewModel() };

        win.Closed += (_, _) => win.DataContext.Dispose();
 
        DependencyInstaller.Unregister<INotificationManager>();
        DependencyInstaller.RegisterSingleton<INotificationManager>(new WindowNotificationManager(win)
        {
            HorizontalAlignment = HorizontalAlignment.Right,
            VerticalAlignment = VerticalAlignment.Bottom,
            Opacity = 0.8,
            Margin = Thickness.Parse("0 0 0 -4")
        });

        return win;
    }
}