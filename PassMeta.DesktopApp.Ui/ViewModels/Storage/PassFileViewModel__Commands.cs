namespace PassMeta.DesktopApp.Ui.ViewModels.Storage
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Common;
    using Common.Interfaces.Services;
    using Common.Models.Entities;
    using Core.Utils;
    using Models.Constants;
    using Splat;

    public partial class PassFileWindowViewModel
    {
        private readonly IDialogService _dialogService = Locator.Current.GetService<IDialogService>()!;
        private readonly IPassFileService _passFileService = Locator.Current.GetService<IPassFileService>()!;
        
        public void Close()
        {
            _close?.Invoke();
            _close = null;
        }
        
        private void Save()
        {
            if (PassFile.Problem is not null)
            {
                _dialogService.ShowFailure(string.Format(Resources.PASSFILE__SOLVE_PROBLEM, PassFile.Problem!.Info));
                return;
            }
            
            if (string.IsNullOrWhiteSpace(Name))
            {
                _dialogService.ShowFailure(Resources.PASSFILE__INCORRECT_NAME);
                return;
            }

            // TODO?
            if (Password == string.Empty || PassFile.Id == 0 && string.IsNullOrEmpty(Password))
            {
                _dialogService.ShowFailure(Resources.PASSFILE__INCORRECT_PASSPHRASE);
                return;
            }

            var pf = PassFile.Copy();
            pf.Name = Name.Trim();
            pf.Color = PassFileColor.List[SelectedColorIndex].Hex;
            pf.PassPhrase = Password ?? pf.PassPhrase;
            pf.Data = new List<PassFile.Section>();

            var result = PassFileLocalManager.UpdateInfo(pf);
            if (result.Ok)
            {
                OnUpdate?.Invoke(result.Data!);
                
                PassFile = result.Data!;
                IsPasswordBoxVisible = false;
            }
        }

        private async Task DeleteAsync()
        {
            var confirm = await _dialogService.ConfirmAsync(
                string.Format(Resources.PASSFILE__CONFIRM_DELETE, PassFile.Name, PassFile.Id));

            if (confirm.Bad) return;

            PassFileLocalManager.Delete(PassFile);
            OnUpdate?.Invoke(null);
        }
        
        private Task MergeAsync()
        {
            // TODO
            return Task.CompletedTask;
        }
    }
}