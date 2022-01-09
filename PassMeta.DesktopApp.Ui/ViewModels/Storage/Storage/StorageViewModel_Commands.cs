namespace PassMeta.DesktopApp.Ui.ViewModels.Storage.Storage
{
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Threading.Tasks;
    using Common;
    using Common.Interfaces.Services;
    using Common.Models.Entities;
    using Common.Utils.Extensions;
    using Components;
    using Core.Utils;
    using Models;
    using Splat;
    using Ui.Utils.Extensions;
    using Utils.Comparers;
    using Views.Main;

    public partial class StorageViewModel
    {
        private readonly IPassFileService _passFileService = Locator.Current.GetService<IPassFileService>()!;
        private readonly IDialogService _dialogService = Locator.Current.GetService<IDialogService>()!;

        private void OnPassFileChangedFromBtn(object? sender, PassFileBtn.PassFileChangedEventArgs ev)
        {
            if (ev.PassFileNew != null) return;
            
            PassFileList.Remove((PassFileBtn)sender!);
        }
        
        private PassFileBtn MakePassFileBtn(PassFile passFile)
        {
            var passFileBtn = new PassFileBtn(passFile, _passFileBarShortMode);
            passFileBtn.PassFileChanged += OnPassFileChangedFromBtn;
            return passFileBtn;
        }

        private async Task _LoadPassFilesAsync(PassFileItemPath lastItemPath)
        {
            using var preloader = MainWindow.Current!.StartPreloader();

            if (!_loaded)
            {
                await _passFileService.RefreshLocalPassFilesAsync();
                _loaded = true;
            }

            _UpdatePassFileList();

            if (lastItemPath.PassFileId is not null)
            {
                PassFilesSelectedIndex = 
                    _passFileList.FindIndex(btn => btn.PassFile!.Id == lastItemPath.PassFileId.Value);
                
                if (PassFilesSelectedIndex >= 0 && lastItemPath.PassFileSectionId is not null)
                {
                    SelectedData.SelectedSectionIndex =
                        SelectedData.SectionsList!.FindIndex(btn => btn.Section.Id == lastItemPath.PassFileSectionId);
                }
            }

            IsPassFilesBarOpened = true;
        }

        private void _UpdatePassFileList()
        {
            var selectedPassFileId = SelectedPassFile?.Id;
            var selectedSectionId = SelectedData.SelectedSection?.Id;

            var list = PassFileLocalManager.GetCurrentList();
            list.Sort(new PassFileComparer());

            var localCreated = list.Where(pf => pf.LocalCreated);
            var localChanged = list.Where(pf => pf.LocalChanged); 
            var unchanged = list.Where(pf => pf.LocalNotChanged);
            var localDeleted = list.Where(pf => pf.LocalDeleted);

            PassFileList = new ObservableCollection<PassFileBtn>(
                localCreated.Concat(localChanged).Concat(unchanged).Concat(localDeleted).Select(MakePassFileBtn));

            PassFilesSelectedIndex = 
                PassFileList.FindIndex(pf => pf.PassFile?.Id == selectedPassFileId);
            
            if (PassFilesSelectedIndex > -1 && selectedSectionId is not null)
            {
                SelectedData.SelectedSectionIndex =
                    SelectedPassFile!.Data?.FindIndex(section => section.Id == selectedSectionId) ?? -1;
            }
        }

        private async Task _DecryptIfRequiredAsync(int _)
        {
            var passFile = SelectedPassFile;
            if (passFile is null || passFile.Data is not null) return;

            using var preloader = MainWindow.Current!.StartPreloader();

            var result = await passFile.AskKeyPhraseAndDecryptAsync();
            if (result.Ok)
            {
                SelectedData.PassFile = passFile;
                return;
            }
            
            PassFilesSelectedIndex = _passFilesPrevSelectedIndex;
        }

        private async Task SaveAsync()
        {
            using var preloader = MainWindow.Current!.StartPreloader();
            
            await _passFileService.ApplyPassFileLocalChangesAsync();
            
            _UpdatePassFileList();
        }

        public async Task PassFileAddAsync()
        {
            using var preloader = MainWindow.Current!.StartPreloader();
            
            var askPassPhrase = await _dialogService.AskPasswordAsync(Resources.STORAGE__ASK_PASSPHRASE_FOR_NEW_PASSFILE);
            if (askPassPhrase.Bad || askPassPhrase.Data == string.Empty) return;
            
            var passFile = PassFileLocalManager.CreateNew(askPassPhrase.Data!);
            var passFileBtn = MakePassFileBtn(passFile);
            
            PassFileList.Insert(0, passFileBtn);
            PassFilesSelectedIndex = 0;
             
            await passFileBtn.OpenAsync();
        }

        public async Task PassFileOpenAsync()
        {
            await SelectedPassFileBtn!.OpenAsync();
        }
    }
}