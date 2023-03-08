using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Controls.Notifications;
using Avalonia.Layout;
using Avalonia.Markup.Xaml;

using Splat;
using System.Threading.Tasks;
using PassMeta.DesktopApp.Common.Abstractions.App;
using PassMeta.DesktopApp.Common.Abstractions.Services;
using PassMeta.DesktopApp.Common.Abstractions.Utils.Logging;
using PassMeta.DesktopApp.Common.Abstractions.Utils.PassMetaClient;
using PassMeta.DesktopApp.Common.Extensions;
using PassMeta.DesktopApp.Ui.App.Observers;
using PassMeta.DesktopApp.Ui.Models.ViewModels.Windows.MainWin;
using PassMeta.DesktopApp.Ui.Views.Windows.MainWin;

namespace PassMeta.DesktopApp.Ui.App;

public class App : Application
{
    private static MainWindow? _mainWindow;
    public static MainWindow? MainWindow
    {
        get => _mainWindow;
        private set
        {
            if (Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                desktop.MainWindow = value;
            }
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
    }

    public static void ReopenMainWindow()
    {
        var window = MakeWindow();
        window.Show();

        MainWindow?.Close();
        MainWindow = window;
    }

    private static async Task BeforeLaunchAsync()
    {
        DependencyInstaller.RegisterServices();
        DependencyInstaller.RegisterViewsForViewModels();
        
        var logManager = Locator.Current.Resolve<ILogsManager>();
        var appConfigManager = Locator.Current.Resolve<IAppConfigManager>();
        var appContextManager = Locator.Current.Resolve<IAppContextManager>();
        var dialogService = Locator.Current.Resolve<IDialogService>();
        var pmClient = Locator.Current.Resolve<IPassMetaClient>();

        logManager.InternalErrorOccured += (_, ev) => 
            dialogService.ShowError(ev.Message, more: ev.Exception.ToString());
        
        appConfigManager.CurrentObservable.Subscribe(new AppConfigObserver());
        pmClient.OnlineObservable.Subscribe(new OnlineObserver());

        await appConfigManager.LoadAsync();
        await appContextManager.LoadAsync();

        if (!await pmClient.CheckConnectionAsync())
        {
            dialogService.ShowInfo(Common.Resources.API__CONNECTION_ERR);
        }

        _ = Task.Run(logManager.CleanUp);
    }

    private static MainWindow MakeWindow()
    {
        var win = new MainWindow { ViewModel = new MainWinModel() };

        win.Activated += (_, _) => Locator.Current.Resolve<IDialogService>().Flush();

        DependencyInstaller.Unregister<INotificationManager>();
        DependencyInstaller.Register<INotificationManager>(new WindowNotificationManager(win)
        {
            HorizontalAlignment = HorizontalAlignment.Right,
            VerticalAlignment = VerticalAlignment.Bottom,
            Opacity = 0.8,
            Margin = Thickness.Parse("0 0 0 -4")
        });

        return win;
    }
}