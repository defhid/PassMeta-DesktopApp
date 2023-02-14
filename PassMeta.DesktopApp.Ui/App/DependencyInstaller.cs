using System.Diagnostics;
using System.Reflection;
using AutoMapper;
using Avalonia.Controls.Notifications;
using PassMeta.DesktopApp.Common.Abstractions.Services;
using PassMeta.DesktopApp.Common.Abstractions.Services.PassFileServices;
using PassMeta.DesktopApp.Common.Abstractions.Services.PassMetaServices;
using PassMeta.DesktopApp.Common.Abstractions.Utils.Logging;
using PassMeta.DesktopApp.Common.Abstractions.Utils.PassMetaClient;
using PassMeta.DesktopApp.Common.Enums;
using PassMeta.DesktopApp.Common.Utils.EntityMapping;
using PassMeta.DesktopApp.Core.Extensions;
using PassMeta.DesktopApp.Core.Services;
using PassMeta.DesktopApp.Core.Services.PassFileServices;
using PassMeta.DesktopApp.Core.Services.PassMetaServices;
using PassMeta.DesktopApp.Core.Utils.Clients;
using PassMeta.DesktopApp.Ui.Interfaces.UiServices;
using PassMeta.DesktopApp.Ui.Services;
using ReactiveUI;
using Splat;

namespace PassMeta.DesktopApp.Ui.App;

public static class DependencyInstaller
{
    public static void RegisterServices()
    {
        var mapper = new Mapper(
            new MapperConfiguration(config => config
                .AddProfile<PassFileProfile>()));
        
        RegisterSingleton<ILogsManager>(
            new LogsManager());

        RegisterSingleton<ILogsWriter>(Resolve<ILogsManager>());

        var dialogService = RegisterSingleton<IDialogService>(
            new DialogService(ResolveOrDefault<INotificationManager>, Resolve<ILogsWriter>()));

        var okBadService = RegisterSingleton<IOkBadService>(
            new OkBadService(dialogService));

        var passMetaClient = RegisterSingleton<IPassMetaClient>(
            new PassMetaClient(Resolve<ILogsWriter>(), dialogService, okBadService));

        var cryptoService = RegisterSingleton<IPassMetaCryptoService>(
            new PassMetaCryptoService(Resolve<ILogsWriter>()));
            
        var passFileRemoteService = RegisterSingleton<IPassFileRemoteService>(
            new PassFileRemoteService(passMetaClient, Resolve<ILogsWriter>()));
            
        var passFileCryptoService = RegisterSingleton<IPassFileCryptoService>(
            new PassFileCryptoService(Resolve<ILogsWriter>()));

        RegisterSingleton<IClipboardService>(
            new ClipboardService(dialogService, Resolve<ILogsWriter>()));

        RegisterSingleton<IAuthService>(
            new AuthService(passMetaClient, dialogService));

        RegisterSingleton<IAccountService>(
            new AccountService(passMetaClient, dialogService));

        RegisterSingleton<IPassFileSyncService>(
            new PassFileSyncService(passFileRemoteService, passMetaClient, dialogService, Resolve<ILogsWriter>()));

        RegisterSingleton<IPassFileImportService>(
            new PassFileImportService(cryptoService, dialogService, Resolve<ILogsWriter>()), PassFileType.Pwd.ToString());

        RegisterSingleton<IPassFileExportService>(
            new PassFileExportService(passFileCryptoService, dialogService, Resolve<ILogsWriter>()), PassFileType.Pwd.ToString());

        RegisterSingleton<IPwdPassFileMergePreparingService>(
            new PwdPassFileMergePreparingService(passFileRemoteService, passFileCryptoService, dialogService));
        
        RegisterSingleton<IPassFileExportUiService>(
            new PassFileExportUiService(Resolve<IPassFileExportService>(), dialogService, Resolve<ILogsWriter>()));
            
        RegisterSingleton<IPassFileMergeUiService>(
            new PassFileMergeUiService(dialogService, Resolve<ILogsWriter>()));
            
        RegisterSingleton<IPassFileRestoreUiService>(
            new PassFileRestoreUiService(dialogService, Resolve<ILogsWriter>()));
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

    private static TContract Resolve<TContract>() 
        where TContract : class
        => Locator.Current.Resolve<TContract>();

    private static TContract? ResolveOrDefault<TContract>() 
        where TContract : class
        => Locator.Current.ResolveOrDefault<TContract>();
}