namespace PassMeta.DesktopApp.Ui.ViewModels.Storage.PassFileLocalListWin
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Reactive.Linq;
    using System.Threading.Tasks;
    using Avalonia.Controls;
    using Common;
    using Common.Constants;
    using Common.Models.Entities;
    using Models;
    using ReactiveUI;
    using Views.Main;

    using ReactCommand = ReactiveUI.ReactiveCommand<System.Reactive.Unit, System.Reactive.Unit>;

    public class PassFileLocalListWinViewModel : ReactiveObject
    {
        private readonly int _currentPassFileId;
        
        public ObservableCollection<DataFile> FoundList { get; } = new()
        {
            new DataFile
            {
                Name = "123.passfile",
                Description = "Lalalalalalalla123",
                PassFileId = 1234
            }
        };

        private int _selectedFileIndex;
        public int SelectedFileIndex
        {
            get => _selectedFileIndex;
            set => this.RaiseAndSetIfChanged(ref _selectedFileIndex, value);
        }
        
        public ReactCommand SelectCommand { get; }
        public ReactCommand ImportCommand { get; }
        public ReactCommand CloseCommand { get; }

        public readonly ViewElements ViewElements = new();

        public PassFileLocalListWinViewModel(PassFile currentPassFile)
        {
            _currentPassFileId = currentPassFile.Id;

            SelectCommand = ReactiveCommand.Create(Select, 
                this.WhenAnyValue(vm => vm.SelectedFileIndex).Select(i => i >= 0));
            
            ImportCommand = ReactiveCommand.CreateFromTask(ImportForeignAsync);
            
            CloseCommand = ReactiveCommand.Create(Close);
        }

        public async Task LoadAsync()
        {
            FoundList.Clear();
            FoundList.Add(new DataFile
            {
                Name = "123.passfile",
                Description = "Lalalalalalalla",
                PassFileId = 123
            });
        }

        private void Select()
        {
            if (SelectedFileIndex < 0) return;

            ViewElements.Window!.Close(FoundList[SelectedFileIndex].FilePath);
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