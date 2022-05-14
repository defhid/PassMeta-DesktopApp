namespace PassMeta.DesktopApp.Ui.ViewModels.Storage.PassFileLocalListWin
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.IO;
    using System.Linq;
    using System.Reactive.Linq;
    using System.Threading.Tasks;
    using Avalonia.Controls;
    using Common;
    using Common.Constants;
    using Common.Models.Entities;
    using Core.Utils;
    using Models;
    using ReactiveUI;
    using Views.Main;

    using ReactCommand = ReactiveUI.ReactiveCommand<System.Reactive.Unit, System.Reactive.Unit>;

    public class PassFileLocalListWinViewModel : ReactiveObject
    {
        private readonly int _currentPassFileId;
        
        public ObservableCollection<DataFile> FoundList { get; } = new();

        private DataFile? _selectedFile;
        public DataFile? SelectedFile
        {
            get => _selectedFile;
            set => this.RaiseAndSetIfChanged(ref _selectedFile, value);
        }

        public ReactCommand SelectCommand { get; }
        public ReactCommand ImportCommand { get; }
        public ReactCommand CloseCommand { get; }

        public readonly ViewElements ViewElements = new();

        public PassFileLocalListWinViewModel(PassFile currentPassFile)
        {
            _currentPassFileId = currentPassFile.Id;

            SelectCommand = ReactiveCommand.Create(Select, 
                this.WhenAnyValue(vm => vm.SelectedFile).Select(file => file is not null));
            
            ImportCommand = ReactiveCommand.CreateFromTask(ImportForeignAsync);
            
            CloseCommand = ReactiveCommand.Create(Close);
        }

        public void Load()
        {
            FoundList.Clear();

            var passFileList = PassFileManager.GetCurrentList();
            var descriptionParts = new Stack<string>();

            foreach (var filePath in Directory.EnumerateFiles(PassFileManager.UserPassFilesPath).OrderBy(x => x))
            {
                var fileName = Path.GetFileName(filePath);
                var isOld = fileName.EndsWith(".old");
                descriptionParts.Clear();

                if (isOld)
                {
                    if (!fileName.EndsWith(ExternalFormat.PassfileEncrypted.FullExtension + ".old"))
                    {
                        continue;
                    }

                    descriptionParts.Push(Resources.PASSFILELIST__DESCRIPTION_OLD_VERSION);
                }
                else if (!fileName.EndsWith(ExternalFormat.PassfileEncrypted.FullExtension))
                {
                    continue;
                }

                if (!int.TryParse(fileName.Split('.').First(), out var passFileId))
                {
                    continue;
                }

                if (passFileId == _currentPassFileId)
                {
                    if (!isOld) continue;
                    descriptionParts.Push(Resources.PASSFILELIST__DESCRIPTION_CURRENT);
                }

                var passFileName = passFileList.FirstOrDefault(pf => pf.Id == passFileId)?.Name;

                descriptionParts.Push(passFileName is null ? Resources.PASSFILELIST__DESCRIPTION_UNKNOWN : $"'{passFileName}'");
                
                FoundList.Add(new DataFile(filePath)
                {
                    Name = fileName,
                    Description = string.Join(", ", descriptionParts),
                });
                
                if (passFileId == _currentPassFileId)
                {
                    SelectedFile = FoundList.Last();
                    ViewElements.DataGrid!.ScrollIntoView(SelectedFile, ViewElements.DataGrid.Columns.First());
                }
            }
        }

        private void Select()
        {
            if (SelectedFile is null) return;

            ViewElements.Window!.Close(SelectedFile.FilePath);
        }

        private async Task ImportForeignAsync()
        {
            var fileDialog = new OpenFileDialog
            {
                AllowMultiple = false,
                Filters = new List<FileDialogFilter>
                {
                    new()
                    {
                        Name = Resources.PASSFILELIST__FILTER_PASSFILES,
                        Extensions = new List<string>
                        {
                            ExternalFormat.PassfileEncrypted.PureExtension, 
                            ExternalFormat.PassfileDecrypted.PureExtension
                        }
                    }
                }
            };
            
            var result = await fileDialog.ShowAsync(MainWindow.Current!);
            if (result?.Any() is true)
            {
                ViewElements.Window!.Close(result.First());
            }
        }
        
        private void Close() => ViewElements.Window!.Close(null);
        
#pragma warning disable 8618
        public PassFileLocalListWinViewModel() {}
#pragma warning restore 8618
    }
}