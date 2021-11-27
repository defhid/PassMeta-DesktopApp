namespace PassMeta.DesktopApp.Ui.ViewModels.Storage
{
    using Common.Interfaces.Services;
    using Splat;

    public partial class PassFileViewModel
    {
        private readonly IDialogService _dialogService = Locator.Current.GetService<IDialogService>()!;
        
        public void Close()
        {
            _close?.Invoke(_passFile);
            _close = null;
        }
        
        private async void Save()
        {
            await _dialogService.ShowInfoAsync("[Save]");
            // TODO
        }
        
        private async void Archive()
        {
            await _dialogService.ShowInfoAsync("[Archive]");
            // TODO
        }

        private async void UnArchive()
        {
            await _dialogService.ShowInfoAsync("[Unarchive]");
            // TODO
        }

        private async void Delete()
        {
            await _dialogService.ShowInfoAsync("[Delete]");
            // TODO

            Close();
        }
    }
}