namespace PassMeta.DesktopApp.Ui.ViewModels.Storage
{
    using DesktopApp.Common;
    using DesktopApp.Common.Interfaces.Services;
    using DesktopApp.Common.Models.Entities;
    using DesktopApp.Core.Utils;
    
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using DynamicData;
    using Models.Components.Storage;
    using Splat;
    using Utils.Extensions;
    using Views.Main;
    using Views.Storage;

    public partial class StorageViewModel
    {
        private readonly IPassFileService _passFileService = Locator.Current.GetService<IPassFileService>()!;
        private readonly IDialogService _dialogService = Locator.Current.GetService<IDialogService>()!;
        
        private async Task _LoadPassFilesAsync()
        {
            if (_userId is null || _userId != AppConfig.Current.User?.Id)
            {
                var list = await _passFileService.GetPassFileListAsync();
                
                _passFiles = list.OrderBy(pf => pf.Name).ToList();
                _userId = AppConfig.Current.User?.Id;
                //TODO: Mode;
                PassFileList = _MakePassFileList();
            }
            
            IsPassFilesBarOpened = true;
        }

        private async Task _DecryptIfRequiredAsync(int _)
        {
            var passFile = SelectedPassFile;
            if (passFile is null || passFile.Data is not null) return;

            var result = await passFile.AskKeyPhraseAndDecryptAsync();
            if (result.Ok)
            {
                _SetPassFileSectionList();
                return;
            }
            
            PassFilesSelectedIndex = _passFilesPrevSelectedIndex;
        }

        private async Task SaveAsync()
        {
            if (_passFiles is null) return;
            
            foreach (var pf in _passFiles)
            {
                if (pf.Problem is not null) continue;
                // TODO: check changed?
                
                await _passFileService.ApplyPassFileLocalChangesAsync();
            }

            // TODO: alert?
        }
        
        #region PassFile

        private async Task PassFileAddAsync()
        {
            var win = new PassFileWindow(new PassFile { Id = 0 });
            
            await win.ShowDialog(MainWindow.Current);
            
            if (win.PassFile is null) return;
            
            _passFiles ??= new List<PassFile>();
            _passFiles.Add(win.PassFile);
            
            PassFileList = _MakePassFileList();
            PassFilesSelectedIndex = _passFiles.Count - 1;
        }

        #endregion
        
        #region Section
        
        private async Task SectionAddAsync()
        {
            var result = await _dialogService.AskStringAsync(Resources.STORAGE__ASK_SECTION_NAME);
            if (result.Bad || result.Data == string.Empty) return;

            var passFile = SelectedPassFile!;
            passFile.Data ??= new List<PassFile.Section>();
            passFile.Data.Add(new PassFile.Section
            {
                Name = result.Data!
            });
            PassFileLocalManager.UpdateData(passFile);

            _SetPassFileSectionList();
            PassFilesSelectedSectionIndex = passFile.Data.Count - 1;
        }

        public async Task SectionRenameAsync()
        {
            var passFile = SelectedPassFile!;
            var sectionBtn = SelectedSectionBtn!;
            var section = sectionBtn.Section;

            var result = await _dialogService.AskStringAsync(Resources.STORAGE__ASK_SECTION_NAME, defaultValue: section.Name);
            if (result.Bad || result.Data == string.Empty || result.Data == section.Name)
            {
                return;
            }

            section.Name = result.Data!;
            PassFileLocalManager.UpdateData(passFile);  // TODO: optimize copying?!

            sectionBtn.Refresh();
        }
        
        public void SectionDelete()
        {
            var passFile = SelectedPassFile!;

            passFile.Data!.RemoveAt(PassFilesSelectedSectionIndex);
            PassFileLocalManager.UpdateData(passFile);
            
            _SetPassFileSectionList();
        }
        
        #endregion
        
        #region Item
        
        private void ItemAdd()
        {
            var sectionBtnList = PassFileSectionItemList!;
            if (sectionBtnList.Any(btn => btn.IsReadOnly))
            {
                _SetPassFileSectionItemList(false);
            }

            var itemBtn = new PassFileSectionItemBtn(new PassFile.Section.Item(), false, ItemDelete, ItemMove);

            PassFileSectionItemList = sectionBtnList.Concat(new[] { itemBtn }).ToArray();
        }

        private void ItemDelete(PassFileSectionItemBtn itemBtn)
        {
            PassFileSectionItemList = PassFileSectionItemList!.Where(btn => !ReferenceEquals(btn, itemBtn)).ToArray();
        }
        
        private void ItemMove(PassFileSectionItemBtn itemBtn, int direction)
        {
            var sectionBtnList = PassFileSectionItemList!;
            var index = sectionBtnList.IndexOf(itemBtn);
            
            if (direction > 0 && index == sectionBtnList.Length - 1 || direction < 0 && index == 0)
            {
                return;
            }

            sectionBtnList[index] = sectionBtnList[index + direction];
            sectionBtnList[index + direction] = itemBtn;

            PassFileSectionItemList = sectionBtnList.ToArray();
        }

        private void ItemsEdit()
        {
            var currentReadOnly = PassFileSectionItemList!.Any(btn => btn.IsReadOnly);
            if (!currentReadOnly)
            {
                SelectedSection!.Items = PassFileSectionItemList!.Select(btn => btn.ToItem()).ToList();
                PassFileLocalManager.UpdateData(SelectedPassFile!);
            }
            
            _SetPassFileSectionItemList(!currentReadOnly);
        }

        #endregion
    }
}