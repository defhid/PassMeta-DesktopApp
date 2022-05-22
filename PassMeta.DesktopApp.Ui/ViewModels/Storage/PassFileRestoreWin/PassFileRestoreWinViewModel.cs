namespace PassMeta.DesktopApp.Ui.ViewModels.Storage.PassFileRestoreWin
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.IO;
    using System.Linq;
    using System.Reactive.Linq;
    using System.Threading.Tasks;
    using Avalonia.Controls;
    using Common.Enums;
    using Common.Utils.Extensions;
    using PassMeta.DesktopApp.Common;
    using PassMeta.DesktopApp.Common.Constants;
    using PassMeta.DesktopApp.Common.Interfaces.Services.PassFile;
    using PassMeta.DesktopApp.Common.Models;
    using PassMeta.DesktopApp.Common.Models.Entities;
    using PassMeta.DesktopApp.Core;
    using PassMeta.DesktopApp.Core.Utils;
    using PassMeta.DesktopApp.Ui.ViewModels.Storage.PassFileRestoreWin.Models;
    using PassMeta.DesktopApp.Ui.Views.Main;
    using ReactiveUI;
    using ReactCommand = ReactiveUI.ReactiveCommand<System.Reactive.Unit, System.Reactive.Unit>;

    public class PassFileRestoreWinViewModel : ReactiveObject
    {
        private readonly int _passFileId;
        private readonly PassFileType _passFileType;
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

        public static IObservable<bool> CanBeDownloaded => PassMetaApi.OnlineSource;

        public readonly ViewElements ViewElements = new();

        public PassFileRestoreWinViewModel(PassFile currentPassFile)
        {
            _passFileId = currentPassFile.Id;
            _passFileType = currentPassFile.Type;
            _ignoreCurrentPath = !currentPassFile.LocalDeleted;

            SelectCommand = ReactiveCommand.Create(Select, 
                this.WhenAnyValue(vm => vm.SelectedFile).Select(file => file is not null));
            
            ImportCommand = ReactiveCommand.CreateFromTask(ImportForeignAsync);
            
            DownloadCommand = ReactiveCommand.CreateFromTask(DownloadAsync);
            
            CloseCommand = ReactiveCommand.Create(Close);
        }

        public void Load()
        {
            FoundList.Clear();

            var passfileExt = _passFileType.ToFileExtension();
            var passFileList = PassFileManager.GetCurrentList(_passFileType);
            var descriptionParts = new Stack<string>();

            foreach (var filePath in Directory.EnumerateFiles(PassFileManager.GetUserPassFilesPath(_passFileType)).OrderBy(x => x))
            {
                var fileName = Path.GetFileName(filePath);
                var isOld = fileName.EndsWith(".old");
                descriptionParts.Clear();

                if (isOld)
                {
                    if (!fileName.EndsWith(ExternalFormat.PwdPassfileEncrypted.FullExtension + ".old"))
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

                if (passFileId == _passFileId)
                {
                    if (!isOld && _ignoreCurrentPath) continue;
                    descriptionParts.Push(Resources.PASSFILELIST__DESCRIPTION_CURRENT);
                }

                var passFileName = passFileList.FirstOrDefault(pf => pf.Id == passFileId)?.Name;

                descriptionParts.Push(passFileName is null ? Resources.PASSFILELIST__DESCRIPTION_UNKNOWN : $"'{passFileName}'");
                
                FoundList.Add(new DataFile(filePath)
                {
                    Name = fileName,
                    Description = string.Join(", ", descriptionParts),
                });
                
                if (passFileId == _passFileId)
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
            var importService = EnvironmentContainer.Resolve<IPassFileImportService>(_passFileType.ToString());
            
            var fileDialog = new OpenFileDialog
            {
                AllowMultiple = false,
                Filters = new List<FileDialogFilter>
                {
                    new()
                    {
                        Name = Resources.PASSFILELIST__FILTER_PASSFILES,
                        Extensions = importService.SupportedFormats.Select(format => format.PureExtension).ToList()
                    }
                }
            };
            
            var result = await fileDialog.ShowAsync(MainWindow.Current!);
            if (result?.Any() is true)
            {
                ViewElements.Window!.Close(Result.Success(result.First()));
            }
        }

        private async Task DownloadAsync()
        {
            var downloadResult = await EnvironmentContainer.Resolve<IPassFileRemoteService>().GetAsync(_passFileId);
            if (downloadResult.Bad) return;

            ViewElements.Window!.Close(Result.Success(downloadResult.Data!));
        }
        
        private void Close() => ViewElements.Window!.Close(Result.Failure<string?>());
        
#pragma warning disable 8618
        public PassFileRestoreWinViewModel() {}
#pragma warning restore 8618
    }
}