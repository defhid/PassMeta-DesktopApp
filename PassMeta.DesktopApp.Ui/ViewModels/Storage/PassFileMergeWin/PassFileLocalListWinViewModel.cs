namespace PassMeta.DesktopApp.Ui.ViewModels.Storage.PassFileMergeWin
{
    using Common.Models.Dto;
    using Models;
    using ReactiveUI;

    using ReactCommand = ReactiveUI.ReactiveCommand<System.Reactive.Unit, System.Reactive.Unit>;

    public class PassFileMergeWinViewModel : ReactiveObject
    {
        public ReactCommand CloseCommand { get; }

        public readonly ViewElements ViewElements = new();

        public PassFileMergeWinViewModel(PassFileMerge merge)
        {
            CloseCommand = ReactiveCommand.Create(Close);
        }

        private void Close() => ViewElements.Window!.Close(null);
        
#pragma warning disable 8618
        public PassFileMergeWinViewModel() {}
#pragma warning restore 8618
    }
}