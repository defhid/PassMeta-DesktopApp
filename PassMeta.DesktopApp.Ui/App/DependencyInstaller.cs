using System.Reflection;
using System.Runtime.CompilerServices;
using AutoMapper;
using Avalonia.Controls.Notifications;
using PassMeta.DesktopApp.Common.Abstractions.App;
using PassMeta.DesktopApp.Common.Abstractions.PassFileContext;
using PassMeta.DesktopApp.Common.Abstractions.Services;
using PassMeta.DesktopApp.Common.Abstractions.Services.PassFileServices;
using PassMeta.DesktopApp.Common.Abstractions.Services.PassMetaServices;
using PassMeta.DesktopApp.Common.Abstractions.Utils;
using PassMeta.DesktopApp.Common.Abstractions.Utils.FileRepository;
using PassMeta.DesktopApp.Common.Abstractions.Utils.Helpers;
using PassMeta.DesktopApp.Common.Abstractions.Utils.Logging;
using PassMeta.DesktopApp.Common.Abstractions.Utils.PassFileContentSerializer;
using PassMeta.DesktopApp.Common.Abstractions.Utils.PassMetaClient;
using PassMeta.DesktopApp.Common.Extensions;
using PassMeta.DesktopApp.Common.Mapping.Entities;
using PassMeta.DesktopApp.Common.Models.App;
using PassMeta.DesktopApp.Core;
using PassMeta.DesktopApp.Core.Services;
using PassMeta.DesktopApp.Core.Services.PassFileServices;
using PassMeta.DesktopApp.Core.Services.PassMetaServices;
using PassMeta.DesktopApp.Core.Utils;
using PassMeta.DesktopApp.Core.Utils.Clients;
using PassMeta.DesktopApp.Core.Utils.FileRepository;
using PassMeta.DesktopApp.Core.Utils.Helpers;
using PassMeta.DesktopApp.Core.Utils.PassFileContentSerializer;
using PassMeta.DesktopApp.Core.Utils.PassFileContext;
using PassMeta.DesktopApp.Ui.Models.Abstractions.Services;
using PassMeta.DesktopApp.Ui.Models.Cache;
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

        Register(AppInfoSource.Get());
        Register(AppLoadingFactory.Create());

        Register(new GeneratorPresetsCache());

        Register<ILogsWriter, ILogsManager>(new AppLogsManager(
            Resolve<AppInfo>().RootPath));

        Register<IDialogService>(new DialogService(
            ResolveOrDefault<INotificationManager>,
            Resolve<ILogsWriter>()));

        Register<IFileDialogService>(new FileDialogService());

        Register<IFileRepositoryFactory>(new FileRepositoryFactory(
            Resolve<AppInfo>().RootPath,
            Resolve<ILogsWriter>()));

        Register<ICounter>(new Counter(
            Resolve<IFileRepositoryFactory>().ForSystemFiles(),
            Resolve<ILogsWriter>()));

        Register<IAppConfigProvider, IAppConfigManager>(new AppConfigManager(
            Resolve<ILogsWriter>(),
            Resolve<IFileRepositoryFactory>().ForSystemFiles()));

        Register<IUserContextProvider, IAppContextProvider, IAppContextManager>(new AppContextManager(
            Resolve<ILogsWriter>(),
            Resolve<IFileRepositoryFactory>().ForSystemFiles()));

        Register<IOkBadService>(new OkBadService(
            Resolve<IDialogService>()));

        Register<IPassMetaClient>(new PassMetaClient(
            Resolve<IAppContextManager>(),
            Resolve<IAppConfigProvider>(),
            Resolve<ILogsWriter>(),
            Resolve<IDialogService>(),
            Resolve<IOkBadService>()));

        Register<IPassMetaCryptoService>(new PassMetaCryptoService());

        Register<IPassMetaRandomService>(new PassMetaRandomService(
            Resolve<ILogsWriter>()));

        Register<IPassMetaInfoService>(new PassMetaInfoService(
            Resolve<IPassMetaClient>()));

        Register<IPassFileContentSerializerFactory>(new PassFileContentSerializerFactory());

        Register<IPassFileLocalStorage>(new PassFileLocalStorage(
            Resolve<IFileRepositoryFactory>(),
            Resolve<ILogsWriter>()));

        Register<IPassFileCryptoService>(new PassFileCryptoService(
            Resolve<IPassMetaCryptoService>(),
            Resolve<IPassFileContentSerializerFactory>(),
            Resolve<ILogsWriter>()));

        Register<IPassFileContextProvider, IPassFileContextManager>(new PassFileContextManager(
            Resolve<IPassFileLocalStorage>(),
            Resolve<IPassFileCryptoService>(),
            Resolve<ICounter>(),
            Resolve<IMapper>(),
            Resolve<IDialogService>(),
            Resolve<ILogsWriter>()));

        Register<IPassPhraseAskHelper>(new PassPhraseAskHelper(
            Resolve<IDialogService>()));

        Register<IPassFileDecryptionHelper>(new PassFileDecryptionHelper(
            Resolve<IPassFileCryptoService>(),
            Resolve<IPassPhraseAskHelper>()));

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

        Register<IHistoryService>(new HistoryService(
            Resolve<IPassMetaClient>()));

        Register<IPassFileSyncService>(new PassFileSyncService(
            Resolve<IPassFileRemoteService>(),
            Resolve<IDialogService>()));

        Register<IPassFileImportService>(new PassFileImportService(
            Resolve<IPassMetaCryptoService>(),
            Resolve<IPassFileContentSerializerFactory>(),
            Resolve<IPassPhraseAskHelper>(),
            Resolve<IDialogService>(),
            Resolve<ILogsWriter>()));

        Register<IPassFileExportService>(new PassFileExportService(
            Resolve<IPassFileContentSerializerFactory>(),
            Resolve<IPassFileCryptoService>(),
            Resolve<IPassFileLocalStorage>(),
            Resolve<IUserContextProvider>(),
            Resolve<IPassPhraseAskHelper>(),
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
            Resolve<IPwdPassFileMergePreparingService>(),
            Resolve<IDialogService>(),
            Resolve<ILogsWriter>()));

        Register<IPassFileRestoreUiService>(new PassFileRestoreUiService(
            Resolve<IDialogService>(),
            Resolve<ILogsWriter>()));

        Resolve<ILogsWriter>().AppConfigProvider = Resolve<IAppConfigProvider>();
    }

    public static void RegisterViewsForViewModels()
    {
        Locator.CurrentMutable.RegisterViewsForViewModels(Assembly.GetExecutingAssembly());
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Register<TContract>(TContract impl, string? contractName = null)
    {
        Locator.CurrentMutable.RegisterConstant(impl, typeof(TContract), contractName);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Register<TContract1, TContract2>(TContract2 impl, string? contractName = null)
    {
        Locator.CurrentMutable.RegisterConstant(impl, typeof(TContract1), contractName);
        Locator.CurrentMutable.RegisterConstant(impl, typeof(TContract2), contractName);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Register<TContract1, TContract2, TContract3>(TContract3 impl, string? contractName = null)
    {
        Locator.CurrentMutable.RegisterConstant(impl, typeof(TContract1), contractName);
        Locator.CurrentMutable.RegisterConstant(impl, typeof(TContract2), contractName);
        Locator.CurrentMutable.RegisterConstant(impl, typeof(TContract3), contractName);
    }

    public static void Unregister<TContract>(string? contractName = null)
    {
        Locator.CurrentMutable.UnregisterCurrent<TContract>(contractName);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static TContract Resolve<TContract>()
        where TContract : class
        => Locator.Current.Resolve<TContract>();

    private static TContract? ResolveOrDefault<TContract>()
        where TContract : class
        => Locator.Current.ResolveOrDefault<TContract>();
}