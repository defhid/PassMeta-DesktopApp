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
            var passFileBtn = new PassFileBtn(passFile, PassFileBarExpander.ShortModeObservable);
            passFileBtn.PassFileChanged += OnPassFileChangedFromBtn;
            return passFileBtn;
        }

        private async Task _LoadPassFilesAsync(PassFileItemPath lastItemPath, bool applyLocalChanges = true)
        {
            using var preloader = MainWindow.Current!.StartPreloader();

            if (!_loaded)
            {
                await _passFileService.RefreshLocalPassFilesAsync(applyLocalChanges);
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

            PassFileBarExpander.IsOpened = true;
        }

        private void _UpdatePassFileList()
        {
            var list = PassFileManager.GetCurrentList();
            list.Sort(new PassFileComparer());

            var localCreated = list.Where(pf => pf.LocalCreated);
            var localChanged = list.Where(pf => pf.LocalChanged); 
            var unchanged = list.Where(pf => pf.LocalNotChanged);
            var localDeleted = list.Where(pf => pf.LocalDeleted);

            PassFileList = new ObservableCollection<PassFileBtn>(
                localCreated.Concat(localChanged).Concat(unchanged).Concat(localDeleted).Select(MakePassFileBtn));
        }

        private async Task _DecryptIfRequiredAndSetSectionsAsync(int _)
        {
            var passFile = SelectedPassFile;
            if (passFile is null || passFile.Data is not null)
            {
                SelectedData.PassFile = passFile;
                return;
            }

            if (!passFile.LocalDeleted)
            {
                using var preloader = MainWindow.Current!.StartPreloader();

                var result = await passFile.LoadIfRequiredAndDecryptAsync();
                if (result.Ok)
                {
                    SelectedData.PassFile = passFile;
                    return;
                }
            }

            PassFilesSelectedIndex = _passFilesPrevSelectedIndex;
        }

        private async Task SaveAsync()
        {
            using (MainWindow.Current!.StartPreloader())
            {
                await _passFileService.ApplyPassFileLocalChangesAsync();
            }

            _loaded = false;
            await _LoadPassFilesAsync(LastItemPath.Copy(), false);
        }

        public async Task PassFileAddAsync()
        {
            using var preloader = MainWindow.Current!.StartPreloader();
            
            var askPassPhrase = await _dialogService.AskPasswordAsync(Resources.STORAGE__ASK_PASSPHRASE_FOR_NEW_PASSFILE);
            if (askPassPhrase.Bad || askPassPhrase.Data == string.Empty) return;
            
            var passFile = PassFileManager.CreateNew(askPassPhrase.Data!);
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