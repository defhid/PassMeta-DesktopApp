namespace PassMeta.DesktopApp.Ui.ViewModels.Storage
{
    using DesktopApp.Common;
    using DesktopApp.Common.Interfaces.Services;
    using DesktopApp.Common.Models;
    using DesktopApp.Common.Models.Entities;
    using DesktopApp.Core.Extensions;
    using DesktopApp.Core.Utils;
    
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using DynamicData;
    using Models.Storage;
    using Splat;
    
    public partial class StorageViewModel
    {
        private readonly IPassFileService _passFileService = Locator.Current.GetService<IPassFileService>()!;
        private readonly IDialogService _dialogService = Locator.Current.GetService<IDialogService>()!;
        
        private async Task _LoadPassFilesAsync()
        {
            if (_userId is null || _userId != AppConfig.Current.User?.Id)
            {
                var result = await _passFileService.GetPassFileListAsync();
                if (result.Bad)
                {
                    _passFiles = null;
                    _userId = null;
                    Mode = null;
                    PassFileList = _MakePassFileList();
                    return;
                }
            
                _passFiles = result.Data;
                _userId = AppConfig.Current.User?.Id;
                Mode = result.Message;
                PassFileList = _MakePassFileList();
            }
            
            IsPassFilesBarOpened = true;
        }

        private async Task _DecryptIfRequiredAsync(int _)
        {
            var passFile = SelectedPassFile;
            if (passFile is null || passFile.IsDecrypted) return;

            var passPhrase = await _dialogService.AskPasswordAsync(Resources.PASSFILE__ASK_PASSPHRASE);
            if (passPhrase.Bad) return;

            passFile.PassPhrase = passPhrase.Data;
            var result = passFile.Decrypt();

            if (result.Ok)
                _SetPassFileSectionList();
            else
                await _dialogService.ShowFailureAsync(result.Message!);
        }

        private async Task SaveAsync()
        {
            if (_passFiles is null) return;
            
            foreach (var pf in _passFiles)
            {
                if (pf.NeedsMergeWith is not null || pf.HasProblem) continue;
                if (!pf.IsChanged) continue;
                
                await _passFileService.SavePassFileAsync(pf);
            }

            // TODO: alert?
        }
        
        #region PassFile

        private async Task PassFileAddAsync()
        {
            var name = await _dialogService.AskStringAsync(Resources.STORAGE__ASK_PASSFILE_NAME);
            if (name.Bad) return;

            Result<string?> passPhrase;
            while (true)
            {
                passPhrase = await _dialogService.AskPasswordAsync(Resources.PASSFILE__ASK_PASSPHRASE);
                if (passPhrase.Bad) return;

                if (string.IsNullOrWhiteSpace(passPhrase.Data!))
                {
                    await _dialogService.ShowFailureAsync(Resources.PASSFILE__INCORRECT_PASSPHRASE);
                    continue;
                }
                
                break;
            }

            _passFiles ??= new List<PassFile>();
            _passFiles.Add(new PassFile
            {
                Id = 0,
                Name = name.Data!,
                Color = null,
                CreatedOn = DateTime.Now,
                ChangedOn = DateTime.Now,
                Version = 1,
                IsArchived = false,
                ChangedLocalOn = DateTime.Now,
                PassPhrase = passPhrase.Data!,
                Data = new List<PassFile.Section>()
            });

            PassFileList = _MakePassFileList();
            PassFilesSelectedIndex = _passFiles.Count - 1;
        }
        
        public async Task PassFileRenameAsync()
        {
            var passFileBtn = SelectedPassFileBtn!;
            var passFile = passFileBtn.PassFile;
            
            var result = await _dialogService.AskStringAsync(Resources.STORAGE__ASK_PASSFILE_NAME, defaultValue: passFile.Name);

            if (result.Bad) return;

            passFile.Name = result.Data!;
            passFile.ChangedLocalOn = DateTime.Now;

            passFileBtn.Refresh();
        }
        
        public async Task PassFileArchiveAsync()
        {
            var passFileBtn = SelectedPassFileBtn!;
            var passFile = passFileBtn.PassFile;
            
            var result = await _dialogService
                .ConfirmAsync(string.Format(Resources.STORAGE__CONFIRM_PASSFILE_ARCHIVE, passFile.Name));

            if (result.Bad) return;

            var archiveResult = await _passFileService.ArchivePassFileAsync(passFile);
            if (archiveResult.Ok)
            {
                passFile.IsArchived = true;
                passFileBtn.Refresh();
            }
        }
        
        public async Task PassFileDeleteAsync()
        {
            var passFileBtn = SelectedPassFileBtn!;
            var passFile = passFileBtn.PassFile;
            
            var accountPassword = await _dialogService
                .AskPasswordAsync(string.Format(Resources.STORAGE__CONFIRM_PASSFILE_DELETE, passFile.Name));

            if (accountPassword.Bad) return;

            var result = await _passFileService.DeletePassFileAsync(passFile, accountPassword.Data!);
            if (result.Ok)
            {
                _passFiles!.Remove(passFile);
                PassFileList = _MakePassFileList();
            }
        }
        
        #endregion
        
        #region Section
        
        private async Task SectionAddAsync()
        {
            var result = await _dialogService
                .AskStringAsync(Resources.STORAGE__ASK_SECTION_NAME);

            if (result.Bad) return;

            var passFile = SelectedPassFile!;
            passFile.Data ??= new List<PassFile.Section>();
            passFile.Data.Add(new PassFile.Section
            {
                Name = result.Data!
            });
            passFile.ChangedLocalOn = DateTime.Now;

            _SetPassFileSectionList();
            PassFilesSelectedSectionIndex = passFile.Data.Count - 1;
        }

        public async Task SectionRenameAsync()
        {
            var passFile = SelectedPassFile!;
            var sectionBtn = SelectedSectionBtn!;
            var section = sectionBtn.Section;

            var result = await _dialogService.AskStringAsync(Resources.STORAGE__ASK_SECTION_NAME, defaultValue: section.Name);
            if (result.Bad || result.Data == section.Name) return;

            section.Name = result.Data!;
            passFile.ChangedLocalOn = DateTime.Now;

            sectionBtn.Refresh();
        }
        
        public void SectionDelete()
        {
            var passFile = SelectedPassFile!;

            passFile.Data!.RemoveAt(PassFilesSelectedSectionIndex);
            passFile.ChangedLocalOn = DateTime.Now;
            
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
                SelectedPassFile!.ChangedLocalOn = DateTime.Now;
            }
            
            _SetPassFileSectionItemList(!currentReadOnly);
        }

        #endregion
    }
}