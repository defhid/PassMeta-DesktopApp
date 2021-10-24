using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using PassMeta.DesktopApp.Common;
using PassMeta.DesktopApp.Common.Interfaces.Services;
using PassMeta.DesktopApp.Common.Models.Entities;
using PassMeta.DesktopApp.Core.Utils;
using Splat;

namespace PassMeta.DesktopApp.Ui.ViewModels.Storage
{
    public partial class StorageViewModel
    {
        private async Task _LoadPassFilesAsync()
        {
            if (_userId is null || _userId != AppConfig.Current.User?.Id)
            {
                if (AppConfig.Current.PassFilesKeyPhrase is null)
                {
                    var passPhrase = await Locator.Current.GetService<IDialogService>()!
                        .AskPasswordAsync(Resources.ASK__PASSPHRASE);
                    if (passPhrase.Bad) return;

                    AppConfig.Current.PassFilesKeyPhrase = passPhrase.Data;
                }
            
                var result = await Locator.Current.GetService<IPassFileService>()!.GetPassFileListAsync();
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

        private async Task SaveAsync()
        {
            Locator.Current.GetService<IDialogService>()!.ShowInfo("SAVING...");
        }

        private async Task PassFileAddAsync()
        {
            var result = await Locator.Current.GetService<IDialogService>()!
                .AskStringAsync(Resources.STORAGE__ASK_PASSFILE_NAME);

            if (result.Bad) return;

            _passFiles ??= new List<PassFile>();
            _passFiles.Add(new PassFile
            {
                Id = 0,
                Name = result.Data!,
                Color = null,
                CreatedOn = DateTime.Now,
                ChangedOn = DateTime.Now,
                Version = 1,
                IsArchived = false,
                ChangedLocalOn = DateTime.Now,
            });

            PassFileList = _MakePassFileList();
            PassFilesSelectedIndex = _passFiles.Count - 1;
        }
        
        private async Task PassFileRenameAsync()
        {
            
        }
        
        private async Task PassFileArchiveAsync()
        {
            
        }
        
        private async Task PassFileDeleteAsync()
        {
            
        }
        
        private async Task SectionAddAsync()
        {
            var result = await Locator.Current.GetService<IDialogService>()!
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

        private async Task SectionRenameAsync()
        {
            
        }
        
        private async Task SectionDeleteAsync()
        {
            
        }
    }
}