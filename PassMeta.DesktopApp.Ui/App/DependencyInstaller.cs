using System.Reflection;
using Avalonia.Controls.Notifications;
using PassMeta.DesktopApp.Common.Abstractions.Services;
using PassMeta.DesktopApp.Common.Abstractions.Services.Logging;
using PassMeta.DesktopApp.Common.Abstractions.Services.PassFile;
using PassMeta.DesktopApp.Common.Abstractions.Utils.PassMetaClient;
using PassMeta.DesktopApp.Common.Enums;
using PassMeta.DesktopApp.Core.Services;
using PassMeta.DesktopApp.Core.Utils.Clients;
using PassMeta.DesktopApp.Ui.Interfaces.UiServices;
using PassMeta.DesktopApp.Ui.Services;
using ReactiveUI;
using Splat;

namespace PassMeta.DesktopApp.Ui.App;

public static class DependencyInstaller
{
    public static void RegisterCoreServices()
    {
        var logService = RegisterSingleton<ILogService>(
            new LogService());

        var dialogService = RegisterSingleton<IDialogService>(
            new DialogService(Resolve<INotificationManager>, logService));

        var okBadService = RegisterSingleton<IOkBadService>(
            new OkBadService(dialogService));

        var passMetaClient = RegisterSingleton<IPassMetaClient>(
            new PassMetaClient(logService, dialogService, okBadService));

        var cryptoService = RegisterSingleton<ICryptoService>(
            new CryptoService(logService));
            
        var passFileRemoteService = RegisterSingleton<IPassFileRemoteService>(
            new PassFileRemoteService(passMetaClient, logService));
            
        var passFileCryptoService = RegisterSingleton<IPassFileCryptoService>(
            new PassFileCryptoService(logService));

        RegisterSingleton<IClipboardService>(
            new ClipboardService(dialogService, logService));

        RegisterSingleton<IAuthService>(
            new AuthService(passMetaClient, dialogService));

        RegisterSingleton<IAccountService>(
            new AccountService(passMetaClient, dialogService));

        RegisterSingleton<IPassFileSyncService>(
            new PassFileSyncService(passFileRemoteService, passMetaClient, dialogService, logService));

        RegisterSingleton<IPassFileImportService>(
            new PassFilePwdImportService(cryptoService, dialogService, logService), PassFileType.Pwd.ToString());

        RegisterSingleton<IPassFileExportService>(
            new PassFilePwdExportService(passFileCryptoService, dialogService, logService), PassFileType.Pwd.ToString());

        RegisterSingleton<IPwdMergePreparingService>(
            new PwdMergePreparingService(passFileRemoteService, passFileCryptoService, dialogService));
    }
        
    public static void RegisterUiServices()
    {
        var dialogService = Resolve<IDialogService>()!;
        var logService = Resolve<ILogService>()!;

        RegisterSingleton<IPassFileExportUiService>(new PassFileExportUiService(dialogService, logService));
            
        RegisterSingleton<IPassFileMergeUiService>(new PassFileMergeUiService(dialogService, logService));
            
        RegisterSingleton<IPassFileRestoreUiService>(new PassFileRestoreUiService(dialogService, logService));
    }

    public static void RegisterViewsForViewModels()
    {
        Locator.CurrentMutable.RegisterViewsForViewModels(Assembly.GetExecutingAssembly());
    }

    public static TContract RegisterSingleton<TContract>(TContract impl, string? contractName = null)
    {
        Locator.CurrentMutable.RegisterConstant(impl, contractName);
        return impl;
    }

    public static void Unregister<TContract>(string? contractName = null)
    {
        Locator.CurrentMutable.UnregisterCurrent<TContract>(contractName);
    }

    private static TContract? Resolve<TContract>() => Locator.Current.GetService<TContract>();
}