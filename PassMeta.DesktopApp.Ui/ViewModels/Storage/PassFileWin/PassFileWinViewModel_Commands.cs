namespace PassMeta.DesktopApp.Ui.ViewModels.Storage.PassFileWin
{
    using System.Threading.Tasks;
    using Common;
    using Common.Interfaces.Services;
    using Constants;
    using Core.Utils;
    using ReactiveUI;
    using Splat;

    public partial class PassFileWinViewModel
    {
        private readonly IDialogService _dialogService = Locator.Current.GetService<IDialogService>()!;
        
        public void Close()
        {
            _closeAction?.Invoke();
            _closeAction = null;
        }

        public async Task ChangePassword()
        {
            // TODO
        }
        
        private void Save()
        {
            if (PassFile.LocalDeleted) return; // TODO
            
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
                Close();
            }
            else
            {
                
            }
        }

        private async Task DeleteAsync()
        {
            var confirm = await _dialogService.ConfirmAsync(
                string.Format(Resources.PASSFILE__CONFIRM_DELETE, PassFile!.Name, PassFile.Id));

            if (confirm.Bad) return;

            PassFile = PassFileManager.Delete(PassFile);
            PassFileChanged = true;

            this.RaisePropertyChanged(nameof(PassFile));
        }
        
        private Task MergeAsync()
        {
            // TODO
            return Task.CompletedTask;
        }
    }
}