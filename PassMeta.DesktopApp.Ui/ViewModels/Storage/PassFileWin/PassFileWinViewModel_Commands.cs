namespace PassMeta.DesktopApp.Ui.ViewModels.Storage.PassFileWin
{
    using System.Threading.Tasks;
    using Common;
    using Common.Interfaces.Services;
    using Constants;
    using Core.Utils;
    using ReactiveUI;
    using Splat;
    using Utils.Extensions;

    public partial class PassFileWinViewModel
    {
        private readonly IDialogService _dialogService = Locator.Current.GetService<IDialogService>()!;
        
        public void Close()
        {
            _closeAction?.Invoke();
            _closeAction = null;
        }

        public async Task ChangePasswordAsync()
        {
            if (PassFile?.LocalDeleted is not false) return;

            var result = await PassFile.LoadIfRequiredAndDecryptAsync();
            if (result.Bad) return;

            var passPhraseNew = await _dialogService.AskPasswordAsync(Resources.PASSFILE__ASK_NEW_PASSPHRASE);
            if (passPhraseNew.Bad || passPhraseNew.Data == string.Empty) return;

            var passfile = PassFile.Copy();
            passfile.PassPhrase = passPhraseNew.Data;
            result = PassFileManager.UpdateData(passfile);

            if (result.Ok)
            {
                PassFile = passfile;
                PassFileChanged = true;
                this.RaisePropertyChanged(nameof(PassFile));
                
                _dialogService.ShowInfo(Resources.PASSFILE__INFO_PASSPHRASE_CHANGED);
            }
            else
            {
                _dialogService.ShowError(result.Message!);
            }
        }
        
        private void Save()
        {
            if (PassFile is null)
            {
                Close();
                return;
            }

            if (PassFile.LocalDeleted)
            {
                return;
            }

            if (string.IsNullOrWhiteSpace(Name))
            {
                _dialogService.ShowFailure(Resources.PASSFILE__INCORRECT_NAME);
                return;
            }

            var passFile = PassFile.Copy();
            passFile.Name = Name.Trim();
            passFile.Color = PassFileColor.List[SelectedColorIndex].Hex;

            var result = PassFileManager.UpdateInfo(passFile);
            if (result.Ok)
            {
                PassFile = passFile;
                PassFileChanged = true;
                this.RaisePropertyChanged(nameof(PassFile));
            }
            else
            {
                _dialogService.ShowError(result.Message!);
            }
        }

        private async Task DeleteAsync()
        {
            if (PassFile?.LocalDeleted is not false) return;

            var confirm = await _dialogService.ConfirmAsync(
                string.Format(Resources.PASSFILE__CONFIRM_DELETE, PassFile!.Name, PassFile.Id));

            if (confirm.Bad) return;

            PassFile = PassFileManager.Delete(PassFile);
            PassFileChanged = true;

            this.RaisePropertyChanged(nameof(PassFile));
            
            if (PassFile is null) Close();
        }

        private Task RestoreAsync()
        {
            _dialogService.ShowInfo("Not implemented...");  // TODO
            return Task.CompletedTask;
        }
        
        private Task MergeAsync()
        {
            _dialogService.ShowInfo("Not implemented...");  // TODO
            return Task.CompletedTask;
        }
    }
}