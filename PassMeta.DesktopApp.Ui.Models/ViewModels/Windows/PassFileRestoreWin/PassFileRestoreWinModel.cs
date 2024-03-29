using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using PassMeta.DesktopApp.Common;
using PassMeta.DesktopApp.Common.Abstractions.App;
using PassMeta.DesktopApp.Common.Abstractions.PassFileContext;
using PassMeta.DesktopApp.Common.Abstractions.Services.PassFileServices;
using PassMeta.DesktopApp.Common.Abstractions.Utils.FileRepository;
using PassMeta.DesktopApp.Common.Abstractions.Utils.PassMetaClient;
using PassMeta.DesktopApp.Common.Constants;
using PassMeta.DesktopApp.Common.Enums;
using PassMeta.DesktopApp.Common.Extensions;
using PassMeta.DesktopApp.Common.Models;
using PassMeta.DesktopApp.Common.Models.Entities.PassFile;
using PassMeta.DesktopApp.Common.Models.Entities.PassFile.Data;
using PassMeta.DesktopApp.Ui.Models.Abstractions.Providers;
using PassMeta.DesktopApp.Ui.Models.ViewModels.Windows.PassFileRestoreWin.Extra;
using ReactiveUI;
using Splat;
using ReactCommand = ReactiveUI.ReactiveCommand<System.Reactive.Unit, System.Reactive.Unit>;

namespace PassMeta.DesktopApp.Ui.Models.ViewModels.Windows.PassFileRestoreWin;

public class PassFileRestoreWinModel : ReactiveObject
{
    private readonly PwdPassFile _passFile;
    private readonly bool _ignoreCurrentPath;
    private DataFile? _selectedFile;
    
    public static IObservable<bool> CanBeDownloaded => Locator.Current.Resolve<IPassMetaClient>().OnlineObservable;

    public readonly ViewElements ViewElements = new();

    public PassFileRestoreWinModel(PwdPassFile passFile)
    {
        _passFile = passFile;
        _ignoreCurrentPath = !passFile.IsLocalDeleted();

        SelectCommand = ReactiveCommand.Create(Select,
            this.WhenAnyValue(vm => vm.SelectedFile).Select(file => file is not null));

        ImportCommand = ReactiveCommand.CreateFromTask(ImportForeignAsync);

        DownloadCommand = ReactiveCommand.CreateFromTask(DownloadAsync);

        CloseCommand = ReactiveCommand.Create(Close);

        Task.Run(LoadAsync);
    }
    
    public ObservableCollection<DataFile> FoundList { get; } = new();

    public DataFile? SelectedFile
    {
        get => _selectedFile;
        set => this.RaiseAndSetIfChanged(ref _selectedFile, value);
    }

    public ReactCommand SelectCommand { get; }
    public ReactCommand ImportCommand { get; }
    public ReactCommand DownloadCommand { get; }
    public ReactCommand CloseCommand { get; }

    public async Task LoadAsync()
    {
        Dispatcher.UIThread.InvokeAsync(() => FoundList.Clear());

        var remoteVersions = await Locator.Current.Resolve<IPassFileRemoteService>().GetVersionsAsync(_passFile.Id);
        // TODO: support for restoring remote versions

        var pmContext = Locator.Current.Resolve<IPassFileContextProvider>().For<PwdPassFile>();

        var passfileExt = '.' + PassFileExternalFormat.Encrypted.Extension;
        var passFileList = pmContext.CurrentList.ToList();
        var descriptionParts = new Stack<string>();

        var userContext = Locator.Current.Resolve<IUserContextProvider>().Current;
        var fileRepository = Locator.Current.Resolve<IFileRepositoryFactory>().ForPassFiles(userContext.UserServerId);
        var files = await fileRepository.GetFilesAsync();

        foreach (var fileName in files.OrderBy(x => x))
        {
            if (!fileName.EndsWith(passfileExt))
            {
                continue;
            }

            descriptionParts.Clear();

            var isLocalCreated = fileName.StartsWith("-");
            if (isLocalCreated)
            {
                descriptionParts.Push(Resources.PASSFILELIST__DESCRIPTION_LOCAL_CREATED);
            }

            if (!int.TryParse(fileName.Split('.').First().Split("v").First(), out var passFileId))
            {
                continue;
            }

            int.TryParse(fileName.Split('.').First().Split("v").Skip(1).FirstOrDefault(), out var passFileVersion);

            var passFile = passFileList.FirstOrDefault(pf => pf.Id == passFileId);

            if (passFile?.Version > passFileVersion)
            {
                descriptionParts.Push(Resources.PASSFILELIST__DESCRIPTION_OLD_VERSION);
            }
            else if (passFile?.Version == passFileVersion)
            {
                descriptionParts.Push(passFile.Id == _passFile.Id
                    ? Resources.PASSFILELIST__DESCRIPTION_CURRENT
                    : Resources.PASSFILELIST__DESCRIPTION_ACTUAL_VERSION );
            }

            descriptionParts.Push(passFile is null
                ? Resources.PASSFILELIST__DESCRIPTION_UNKNOWN
                : $"'{passFile.Name}'");

            var dataFile = new DataFile(fileRepository.GetAbsolutePath(fileName))
            {
                PassFileId = passFileId,
                PassFileVersion = passFileVersion,
                Name = fileName,
                Description = string.Join(", ", descriptionParts),
            };

            await Dispatcher.UIThread.InvokeAsync(() => FoundList.Add(dataFile));
        }

        await Dispatcher.UIThread.InvokeAsync(() =>
            SelectedFile = FoundList.FirstOrDefault(x =>
                x.PassFileId == _passFile.Id &&
                x.PassFileVersion == _passFile.Version));
        
        // TODO DataGrid!.ScrollIntoView(SelectedFile, DataGrid.Columns.First());
    }

    private void Select()
    {
        if (SelectedFile is null) return;

        ViewElements.Window!.Close(Result.Success(SelectedFile.FilePath));
    }

    private async Task ImportForeignAsync()
    {
        var importService = Locator.Current.Resolve<IPassFileImportService>();
        var storageProvider = Locator.Current.Resolve<IHostWindowProvider>().Window.StorageProvider;

        var files = await storageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            FileTypeFilter = new[]
            {
                new FilePickerFileType(Resources.PASSFILELIST__FILTER_PASSFILES)
                {
                    Patterns = importService.GetSupportedFormats(PassFileType.Pwd)
                        .Select(x => "*." + x.Extension)
                        .ToList()
                }
            }
        });

        if (files.Count == 1)
        {
            ViewElements.Window!.Close(files[0].Path.AbsolutePath);
        }
    }

    private async Task DownloadAsync()
    {
        var remoteService = Locator.Current.Resolve<IPassFileRemoteService>();

        var infoResult = await remoteService.GetInfoAsync(_passFile);
        if (infoResult.Bad)
        {
            return;
        }

        var passFile = infoResult.Data!;
        var result = await remoteService.GetEncryptedContentAsync(passFile.Id, passFile.Version);
        passFile.Content = new PassFileContent<List<PwdSection>>(result.Data!);

        if (passFile.Content.Encrypted is not null)
        {
            ViewElements.Window!.Close(Result.Success(passFile));
        }
    }

    private void Close() => ViewElements.Window!.Close(Result.Failure<string?>());
}