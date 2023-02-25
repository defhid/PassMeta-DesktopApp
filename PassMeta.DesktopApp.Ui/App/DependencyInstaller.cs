using System.Reflection;
using AutoMapper;
using Avalonia.Controls.Notifications;
using PassMeta.DesktopApp.Common.Abstractions.AppConfig;
using PassMeta.DesktopApp.Common.Abstractions.AppContext;
using PassMeta.DesktopApp.Common.Abstractions.PassFileContext;
using PassMeta.DesktopApp.Common.Abstractions.Services;
using PassMeta.DesktopApp.Common.Abstractions.Services.PassFileServices;
using PassMeta.DesktopApp.Common.Abstractions.Services.PassMetaServices;
using PassMeta.DesktopApp.Common.Abstractions.Utils;
using PassMeta.DesktopApp.Common.Abstractions.Utils.FileRepository;
using PassMeta.DesktopApp.Common.Abstractions.Utils.Logging;
using PassMeta.DesktopApp.Common.Abstractions.Utils.PassFileContentSerializer;
using PassMeta.DesktopApp.Common.Abstractions.Utils.PassMetaClient;
using PassMeta.DesktopApp.Common.Mapping.Entities;
using PassMeta.DesktopApp.Core.Extensions;
using PassMeta.DesktopApp.Core.Services;
using PassMeta.DesktopApp.Core.Services.PassFileServices;
using PassMeta.DesktopApp.Core.Services.PassMetaServices;
using PassMeta.DesktopApp.Core.Utils;
using PassMeta.DesktopApp.Core.Utils.Clients;
using PassMeta.DesktopApp.Core.Utils.FileRepository;
using PassMeta.DesktopApp.Core.Utils.PassFileContentSerializer;
using PassMeta.DesktopApp.Ui.Interfaces.Services;
using PassMeta.DesktopApp.Ui.Services;
using ReactiveUI;
using Splat;

namespace PassMeta.DesktopApp.Ui.App;

public static class DependencyInstaller
{
    public static void RegisterServices()
    {
        Register<IMapper>(new Mapper(new MapperConfiguration(config => config
            .AddProfile<PassFileProfile>())));

        Register<ILogsWriter, ILogsManager>(new LogsManager());

        Register<IDialogService>(new DialogService(
            ResolveOrDefault<INotificationManager>,
            Resolve<ILogsWriter>()));

        Register<IFileRepositoryFactory>(new FileRepositoryFactory(
            Resolve<ILogsWriter>()));

        Register<IAppConfigProvider, IAppConfigManager>(new AppConfigManager(
            Resolve<ILogsWriter>(),
            Resolve<IFileRepositoryFactory>()));

        Register<IAppContextProvider, IAppContextManager>(new AppContextManager(
            Resolve<ILogsWriter>(),
            Resolve<IFileRepositoryFactory>()));

        Register<IOkBadService>(new OkBadService(
            Resolve<IDialogService>()));

        Register<IPassMetaClient>(new PassMetaClient(
            Resolve<IAppContextManager>(),
            Resolve<IAppConfigProvider>(),
            Resolve<ILogsWriter>(),
            Resolve<IDialogService>(),
            Resolve<IOkBadService>()));

        Register<IPassMetaCryptoService>(new PassMetaCryptoService());

        Register<IPassFileContentSerializerFactory>(new PassFileContentSerializerFactory());

        Register<IPassFileLocalStorage>(new PassFileLocalStorage(
            Resolve<IFileRepositoryFactory>(),
            Resolve<ILogsWriter>()));

        Register<IPassFileCryptoService>(new PassFileCryptoService(
            Resolve<IPassMetaCryptoService>(),
            Resolve<IPassFileContentSerializerFactory>(),
            Resolve<ILogsWriter>()));

        Register<IPassFileRemoteService>(new PassFileRemoteService(
            Resolve<IPassMetaClient>(),
            Resolve<IMapper>(),
            Resolve<IDialogService>(),
            Resolve<ILogsWriter>()));

        Register<IClipboardService>(new ClipboardService(
            Resolve<IDialogService>(),
            Resolve<ILogsWriter>()));

        Register<IAuthService>(new AuthService(
            Resolve<IPassMetaClient>(),
            Resolve<IAppContextManager>(),
            Resolve<IDialogService>()));

        Register<IAccountService>(new AccountService(
            Resolve<IPassMetaClient>(),
            Resolve<IAppContextManager>(),
            Resolve<IDialogService>()));

        Register<IPassFileSyncService>(new PassFileSyncService(
            Resolve<IPassFileContextProvider>(),
            Resolve<IPassFileRemoteService>(),
            Resolve<IDialogService>()));

        Register<IPassFileImportService>(new PassFileImportService(
            Resolve<IPassMetaCryptoService>(),
            Resolve<IPassFileContentSerializerFactory>(),
            Resolve<IDialogService>(),
            Resolve<ILogsWriter>()));

        Register<IPassFileExportService>(new PassFileExportService(
            Resolve<IPassFileContentSerializerFactory>(),
            Resolve<IPassFileCryptoService>(),
            Resolve<IPassFileLocalStorage>(),
            Resolve<IUserContextProvider>(),
            Resolve<IDialogService>(), 
            Resolve<ILogsWriter>()));

        Register<IPwdPassFileMergePreparingService>(new PwdPassFileMergePreparingService(
            Resolve<IPassFileContextProvider>(),
            Resolve<IPassFileRemoteService>(),
            Resolve<IPassFileCryptoService>(),
            Resolve<IDialogService>()));

        Register<IPassFileExportUiService>(new PassFileExportUiService(
            Resolve<IPassFileExportService>(),
            Resolve<IDialogService>(),
            Resolve<ILogsWriter>()));

        Register<IPassFileMergeUiService>(new PassFileMergeUiService(
            Resolve<IDialogService>(),
            Resolve<ILogsWriter>()));

        Register<IPassFileRestoreUiService>(new PassFileRestoreUiService(
            Resolve<IDialogService>(),
            Resolve<ILogsWriter>()));
    }

    public static void RegisterViewsForViewModels()
    {
        Locator.CurrentMutable.RegisterViewsForViewModels(Assembly.GetExecutingAssembly());
    }

    public static void Register<TContract>(TContract impl, string? contractName = null)
    {
        Locator.CurrentMutable.RegisterConstant<TContract>(impl, contractName);
    }

    public static void Register<TContract1, TContract2>(TContract2 impl, string? contractName = null)
        where TContract2 : TContract1
    {
        Locator.CurrentMutable.RegisterConstant<TContract1>(impl, contractName);
        Locator.CurrentMutable.RegisterConstant<TContract2>(impl, contractName);
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