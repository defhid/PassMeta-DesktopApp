using System.Threading.Tasks;
using System.Windows.Input;
using Avalonia.Controls;
using PassMeta.DesktopApp.Common;
using PassMeta.DesktopApp.Common.Interfaces.Services;
using PassMeta.DesktopApp.Core.Utils;
using Splat;

namespace PassMeta.DesktopApp.Ui.ViewModels.Storage
{
    public partial class StorageViewModel
    {
        #region Commands
        
        public ICommand SaveCommand { get; }
        
        public ICommand AddPassFileCommand { get; }
        public ICommand RenamePassFileCommand { get; }
        public ICommand ArchivePassFileCommand { get; }
        public ICommand DeletePassFileCommand { get; }
        
        public ICommand AddSectionCommand { get; }
        public ICommand RenameSectionCommand { get; }
        public ICommand DeleteSectionCommand { get; }
        
        #endregion

        private async Task _LoadPassFilesAsync()
        {
            if (AppConfig.Current.PassFilesKeyPhrase is null)
            {
                var passPhrase = await Locator.Current.GetService<IDialogService>()!
                    .AskString(Resources.ASK__PASSPHRASE);
                if (passPhrase.Bad) return;

                AppConfig.Current.PassFilesKeyPhrase = passPhrase.Data;
            }
            
            var result = await Locator.Current.GetService<IPassFileService>()!.GetPassFileListAsync();
            if (result.Bad) return;
            
            _passFiles = result.Data;
            Mode = result.Message;
            PassFileList = _MakePassFileList();
            IsPassFilesBarOpened = true;
        }

        private async Task _SaveAsync()
        {
            Locator.Current.GetService<IDialogService>()!.ShowInfo("SAVING...");
        }

        private async Task _PassFileAddAsync()
        {
            Locator.Current.GetService<IDialogService>()!.ShowInfo("Adding...");
        }
        
        private async Task _PassFileRenameAsync(MenuItem menuItem)
        {
            
        }
        
        private async Task _PassFileArchiveAsync(MenuItem menuItem)
        {
            
        }
        
        private async Task _PassFileDeleteAsync(MenuItem menuItem)
        {
            
        }
        
        private async Task _SectionAddAsync()
        {
            
        }

        private async Task _SectionRenameAsync(MenuItem menuItem)
        {
            
        }
        
        private async Task _SectionDeleteAsync(MenuItem menuItem)
        {
            
        }
    }
}