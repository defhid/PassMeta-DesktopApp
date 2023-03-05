using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using PassMeta.DesktopApp.Common;
using PassMeta.DesktopApp.Common.Abstractions.App;
using PassMeta.DesktopApp.Common.Abstractions.PassFileContext;
using PassMeta.DesktopApp.Common.Abstractions.Services;
using PassMeta.DesktopApp.Common.Abstractions.Services.PassFileServices;
using PassMeta.DesktopApp.Common.Abstractions.Utils.FileRepository;
using PassMeta.DesktopApp.Common.Abstractions.Utils.PassMetaClient;
using PassMeta.DesktopApp.Common.Constants;
using PassMeta.DesktopApp.Common.Enums;
using PassMeta.DesktopApp.Common.Extensions;
using PassMeta.DesktopApp.Common.Models;
using PassMeta.DesktopApp.Common.Models.Entities.PassFile;
using PassMeta.DesktopApp.Common.Models.Entities.PassFile.Data;
using PassMeta.DesktopApp.Ui.Models.ViewModels.Windows.PassFileRestoreWin.Extra;
using ReactiveUI;
using Splat;
using ReactCommand = ReactiveUI.ReactiveCommand<System.Reactive.Unit, System.Reactive.Unit>;

namespace PassMeta.DesktopApp.Ui.Models.ViewModels.Windows.PassFileRestoreWin;

public class PassFileRestoreWinModel : ReactiveObject
{
    private readonly PwdPassFile _passFile;
    private readonly bool _ignoreCurrentPath;

    public ObservableCollection<DataFile> FoundList { get; } = new();

    private DataFile? _selectedFile;

    public DataFile? SelectedFile
    {
        get => _selectedFile;
        set => this.RaiseAndSetIfChanged(ref _selectedFile, value);
    }

    public ReactCommand SelectCommand { get; }
    public ReactCommand ImportCommand { get; }
    public ReactCommand DownloadCommand { get; }
    public ReactCommand CloseCommand { get; }

    public static IObservable<bool> CanBeDownloaded => Locator.Current.Resolve<IPassMetaClient>().OnlineObservable;

    public readonly ViewElements ViewElements = new();
    
    /// <summary>DEV constructor.</summary>
    public PassFileRestoreWinModel()
    {
    }

    public PassFileRestoreWinModel(PwdPassFile passFile)
    {
        _passFile = passFile;
        _ignoreCurrentPath = !passFile.IsLocalDeleted();

        SelectCommand = ReactiveCommand.Create(Select,
            this.WhenAnyValue(vm => vm.SelectedFile).Select(file => file is not null));

        ImportCommand = ReactiveCommand.CreateFromTask(ImportForeignAsync);

        DownloadCommand = ReactiveCommand.CreateFromTask(DownloadAsync);

        CloseCommand = ReactiveCommand.Create(Close);
    }

    public async Task LoadAsync()
    {
        FoundList.Clear();

        var remoteVersions = await Locator.Current.Resolve<IPassFileRemoteService>().GetVersionsAsync(_passFile.Id);
        // TODO: support for restoring remote versions

        var pmContext = Locator.Current.Resolve<IPassFileContextProvider>().For<PwdPassFile>();

        var passfileExt = '.' + PassFileExternalFormat.Encrypted.Extension;
        var passFileList = pmContext.CurrentList.ToList();
        var descriptionParts = new Stack<string>();

        var userContext = Locator.Current.Resolve<IUserContextProvider>().Current;
        var fileRepository = Locator.Current.Resolve<IFileRepositoryFactory>().ForPassFiles(userContext.UserServerId);
        var files = await fileRepository.GetFilesAsync();

        foreach (var filePath in files.OrderBy(x => x))
        {
            var fileName = Path.GetFileName(filePath);
            var isOld = fileName.EndsWith(".old");
            descriptionParts.Clear();

            if (isOld)
            {
                if (!fileName.EndsWith(passfileExt + ".old"))
                {
                    continue;
                }

                descriptionParts.Push(Resources.PASSFILELIST__DESCRIPTION_OLD_VERSION);
            }
            else if (!fileName.EndsWith(passfileExt))
            {
                continue;
            }

            if (!int.TryParse(fileName.Split('.').First(), out var passFileId))
            {
                continue;
            }

            if (passFileId == _passFile.Id)
            {
                if (!isOld && _ignoreCurrentPath) continue;
                descriptionParts.Push(Resources.PASSFILELIST__DESCRIPTION_CURRENT);
            }

            var passFileName = passFileList.FirstOrDefault(pf => pf.Id == passFileId)?.Name;

            descriptionParts.Push(passFileName is null
                ? Resources.PASSFILELIST__DESCRIPTION_UNKNOWN
                : $"'{passFileName}'");

            FoundList.Add(new DataFile(filePath)
            {
                Name = fileName,
                Description = string.Join(", ", descriptionParts),
            });

            if (passFileId == _passFile.Id)
            {
                SelectedFile = FoundList.Last();
                ViewElements.DataGrid!.ScrollIntoView(SelectedFile, ViewElements.DataGrid.Columns.First());
            }
        }
    }

    private void Select()
    {
        if (SelectedFile is null) return;

        ViewElements.Window!.Close(Result.Success(SelectedFile.FilePath));
    }

    private async Task ImportForeignAsync()
    {
        var importService = Locator.Current.Resolve<IPassFileImportService>();
        var dialogService = Locator.Current.Resolve<IFileDialogService>();

        var extensions = importService.GetSupportedFormats(PassFileType.Pwd)
            .Select(format => format.Extension)
            .ToList();

        var result = await dialogService.AskForReadingAsync(new[]
        {
            (Resources.PASSFILELIST__FILTER_PASSFILES, extensions)
        });

        if (result.Ok)
        {
            ViewElements.Window!.Close(result);
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