namespace PassMeta.DesktopApp.Ui.ViewModels.Storage.PassFileMergeWin.Components
{
    using System;
    using System.Linq;
    using Common.Models.Entities;
    using ReactiveUI;
    
    using ReactCommand = ReactiveUI.ReactiveCommand<System.Reactive.Unit, System.Reactive.Unit>;

    public class ItemBtn : ReactiveObject
    {
        public string? What { get; set; }
        public string? Password { get; set; }
        public string? Comment { get; set; }

        public ReactCommand DeleteCommand { get; }
        public ReactCommand UpCommand { get; }
        public ReactCommand DownCommand { get; }
        public ReactCommand TransferCommand { get; }

        public ItemBtn(PassFile.PwdSection.PwdItem item,
            Action<ItemBtn> onDelete,
            Action<ItemBtn, int> onMove,
            Action<ItemBtn> onTransfer)
        {
            What = string.Join('\n', item.What.Select(x => x.Trim()).Where(x => x != string.Empty));
            Password = item.Password;
            Comment = item.Comment;

            DeleteCommand = ReactiveCommand.Create(() => onDelete(this));
            UpCommand = ReactiveCommand.Create(() => onMove(this, -1));
            DownCommand = ReactiveCommand.Create(() => onMove(this, 1));
            TransferCommand = ReactiveCommand.Create(() => onTransfer(this));
        }

        public PassFile.PwdSection.PwdItem ToItem() => new()
        {
            What = _NormalizeWhat().Split('\n'),
            Password = Password ?? string.Empty,
            Comment = Comment?.Trim() ?? string.Empty
        };

        private string _NormalizeWhat()
        {
            return string.IsNullOrWhiteSpace(What) 
                ? string.Empty 
                : string.Join('\n', What.Split('\n').Select(x => x.Trim()).Where(x => x != string.Empty));
        }
    }
}