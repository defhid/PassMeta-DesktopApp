namespace PassMeta.DesktopApp.Ui.ViewModels.Storage
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Common;
    using Common.Interfaces.Services;
    using Common.Models.Entities;
    using Models.Constants;
    using Splat;
    using Utils.Extensions;

    public partial class PassFileViewModel
    {
        private readonly IDialogService _dialogService = Locator.Current.GetService<IDialogService>()!;
        private readonly IPassFileService _passFileService = Locator.Current.GetService<IPassFileService>()!;
        
        public void Close()
        {
            _close?.Invoke();
            _close = null;
        }
        
        private async Task SaveAsync()
        {
            if (!await _CheckPassFileProblemAsync()) return;
            
            if (string.IsNullOrWhiteSpace(Name))
            {
                await _dialogService.ShowFailureAsync(Resources.PASSFILE__INCORRECT_NAME);
                return;
            }

            if (Password == string.Empty || PassFile.Id == 0 && string.IsNullOrEmpty(Password))
            {
                await _dialogService.ShowFailureAsync(Resources.PASSFILE__INCORRECT_PASSPHRASE);
                return;
            }
            
            if (PassFile.Id > 0 && !PassFile.IsDecrypted)
            {
                var decryptResult = await PassFile.AskKeyPhraseAndDecryptAsync();
                if (decryptResult.Bad) return;
            }

            var pf = PassFile.Copy();
            pf.Name = Name.Trim();
            pf.Color = PassFileColor.List[SelectedColorIndex].Hex;
            pf.PassPhrase = Password ?? pf.PassPhrase;
            pf.Data = new List<PassFile.Section>();
            
            var result = await _passFileService.SavePassFileAsync(pf);
            if (result.Ok)
            {
                OnUpdate?.Invoke(result.Data!);
                
                PassFile = result.Data!;
                IsPasswordBoxVisible = false;
            }
        }
        
        private async Task ArchiveAsync()
        {
            if (!await _CheckPassFileProblemAsync()) return;
            
            var confirm = await _dialogService
                .ConfirmAsync(string.Format(Resources.PASSFILE__CONFIRM_ARCHIVE, PassFile.Name));

            if (confirm.Bad) return;

            var result = await _passFileService.ArchivePassFileAsync(PassFile);
            if (result.Ok)
                PassFile = result.Data!;
        }

        private async Task UnArchiveAsync()
        {
            if (!await _CheckPassFileProblemAsync()) return;
            
            var result = await _passFileService.UnArchivePassFileAsync(PassFile);
            if (result.Ok)
                PassFile = result.Data!;
        }

        private async Task DeleteAsync()
        {
            var accountPassword = await _dialogService
                .AskPasswordAsync(string.Format(Resources.PASSFILE__CONFIRM_DELETE, PassFile.Name));

            if (accountPassword.Bad || accountPassword.Data == string.Empty) return;

            var result = await _passFileService.DeletePassFileAsync(PassFile, accountPassword.Data!);
            if (result.Bad) return;
            
            OnUpdate?.Invoke(null);
        }
        
        private Task MergeAsync()
        {
            // TODO
            return Task.CompletedTask;
        }

        private async Task<bool> _CheckPassFileProblemAsync()
        {
            if (!PassFile.HasProblem) return true;
            
            await _dialogService.ShowFailureAsync(
                string.Format(Resources.PASSFILE__SOLVE_PROBLEM, PassFile.Problem!.Info));

            return false;
        }
    }
}