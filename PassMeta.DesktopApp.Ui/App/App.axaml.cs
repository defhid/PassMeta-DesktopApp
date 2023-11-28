using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Controls.Notifications;
using Avalonia.Layout;
using Avalonia.Markup.Xaml;
using PassMeta.DesktopApp.Common.Abstractions.App;
using PassMeta.DesktopApp.Common.Abstractions.Services;
using PassMeta.DesktopApp.Common.Abstractions.Services.PassMetaServices;
using PassMeta.DesktopApp.Common.Abstractions.Utils.Logging;
using PassMeta.DesktopApp.Common.Abstractions.Utils.PassMetaClient;
using PassMeta.DesktopApp.Common.Enums;
using PassMeta.DesktopApp.Common.Extensions;
using PassMeta.DesktopApp.Ui.App.Observers;
using PassMeta.DesktopApp.Ui.Models.Abstractions.Providers;
using PassMeta.DesktopApp.Ui.Models.Providers;
using PassMeta.DesktopApp.Ui.Models.ViewModels.Windows.MainWin;
using PassMeta.DesktopApp.Ui.Views.Windows.MainWin;
using Splat;

namespace PassMeta.DesktopApp.Ui.App;

public class App : Application
{
    private static MainWindow? _mainWindow;
    private Task? _beforeLaunchTask;

    /// <inheritdoc />
    public override void RegisterServices()
    {
        base.RegisterServices();
        DependencyInstaller.RegisterServices();
        DependencyInstaller.RegisterPageViews();
    }

    /// <inheritdoc />
    public override void Initialize()
    {
        _beforeLaunchTask = Task.Run(BeforeLaunchAsync);
        _beforeLaunchTask.ConfigureAwait(false);
        AvaloniaXamlLoader.Load(this);
    }

    /// <inheritdoc />
    public override void OnFrameworkInitializationCompleted()
    {
        _beforeLaunchTask!.GetAwaiter().GetResult();
        MainWindow = MakeWindow();
    }

    /// <summary></summary>
    public static void ReopenMainWindow()
    {
        var window = MakeWindow();
        window.Show();

        MainWindow?.Close();
        MainWindow = window;
    }

    /// <summary></summary>
    private static MainWindow? MainWindow
    {
        get => _mainWindow;
        set
        {
            if (Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                desktop.MainWindow = value;
            }
            _mainWindow = value;
        }
    }

    private static async Task BeforeLaunchAsync()
    {
        var logManager = Locator.Current.Resolve<ILogsManager>();
        var appConfigManager = Locator.Current.Resolve<IAppConfigManager>();
        var appContextManager = Locator.Current.Resolve<IAppContextManager>();
        var appPresetsManager = Locator.Current.Resolve<IAppPresetsManager>();
        var userContextProvider = Locator.Current.Resolve<IUserContextProvider>();
        var dialogService = Locator.Current.Resolve<IDialogService>();
        var pmClient = Locator.Current.Resolve<IPassMetaClient>();
        var pmInfoService = Locator.Current.Resolve<IPassMetaInfoService>();

        logManager.InternalErrorOccured += (_, ev) => 
            dialogService.ShowError(ev.Message, more: ev.Exception.ToString(), defaultPresenter: DialogPresenter.Window);

        await appConfigManager.LoadAsync();
        await appContextManager.LoadAsync();
        
        var result = await pmInfoService.LoadAsync();
        if (result.Ok)
        {
            await appContextManager.RefreshFromAsync(result.Data!);
        }

        if (!pmClient.Online)
        {
            dialogService.ShowInfo(Common.Resources.API__CONNECTION_ERR);
        }

        appConfigManager.CurrentObservable.Subscribe(new AppConfigObserver());
        pmClient.OnlineObservable.Subscribe(new OnlineObserver());
        userContextProvider.CurrentObservable.Subscribe(new UserContextObserver());

        _ = Task.Run(logManager.CleanUpAsync);
        _ = Task.Run(appPresetsManager.LoadAsync);
    }

    private static MainWindow MakeWindow()
    {
        var win = new MainWindow { ViewModel = new MainWinModel() };

        win.GotFocus += (_, _) => Locator.Current.Resolve<IDialogService>().Flush();
        win.Activated += (_, _) =>
        {
            DependencyInstaller.Unregister<IHostWindowProvider>();
            DependencyInstaller.Register<IHostWindowProvider>(new SimpleHostWindowProvider(win));

            DependencyInstaller.Unregister<INotificationManager>();
            DependencyInstaller.Register<INotificationManager>(new WindowNotificationManager(win)
            {
                HorizontalAlignment = HorizontalAlignment.Right,
                VerticalAlignment = VerticalAlignment.Bottom,
                Opacity = 0.8,
                Margin = Thickness.Parse("0 0 0 -4")
            });
        };

        return win;
    }
}