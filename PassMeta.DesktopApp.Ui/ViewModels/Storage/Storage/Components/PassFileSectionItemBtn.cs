namespace PassMeta.DesktopApp.Ui.ViewModels.Storage.Storage.Components
{
    using System;
    using System.Linq;
    using System.Reactive.Linq;
    using System.Threading.Tasks;
    using Common;
    using Common.Interfaces.Services;
    using Common.Models.Entities;
    using Core;
    using ReactiveUI;
    
    using ReactCommand = ReactiveUI.ReactiveCommand<System.Reactive.Unit, System.Reactive.Unit>;

    public class PassFileSectionItemBtn : ReactiveObject
    {
        private readonly IDialogService _dialogService = EnvironmentContainer.Resolve<IDialogService>();
        
        private readonly ObservableAsPropertyHelper<bool> _isReadOnly;
        public bool IsReadOnly => _isReadOnly.Value;
        
        public string? What { get; set; }
        public string? Password { get; set; }
        public string? Comment { get; set; }

        public IObservable<bool> IsCommentTextVisible { get; }
        public IObservable<bool> IsCommentInputVisible { get; }
        
        public ReactCommand CopyWhatCommand { get; }
        public ReactCommand CopyPasswordCommand { get; }
        public ReactCommand DeleteCommand { get; }
        public ReactCommand UpCommand { get; }
        public ReactCommand DownCommand { get; }

        public PassFileSectionItemBtn(PassFile.Section.Item item,
            IObservable<bool> editModeObservable,
            Action<PassFileSectionItemBtn> onDelete,
            Action<PassFileSectionItemBtn, int> onMove)
        {
            What = string.Join('\n', item.What.Select(x => x.Trim()).Where(x => x != string.Empty));
            Password = item.Password;
            Comment = item.Comment;

            _isReadOnly = editModeObservable.Select(editMode => !editMode)
                .ToProperty(this, nameof(IsReadOnly));
            
            IsCommentTextVisible = this.WhenAnyValue(btn => btn.IsReadOnly, btn => btn.Comment)
                .Select(pair => pair.Item1 && pair.Item2 != string.Empty);
            
            IsCommentInputVisible = this.WhenAnyValue(btn => btn.IsReadOnly)
                .Select(isReadOnly => !isReadOnly);

            CopyWhatCommand = ReactiveCommand.CreateFromTask(_CopyWhatAsync);
            CopyPasswordCommand = ReactiveCommand.CreateFromTask(_CopyPasswordAsync);
            
            DeleteCommand = ReactiveCommand.Create(() => onDelete(this));
            UpCommand = ReactiveCommand.Create(() => onMove(this, -1));
            DownCommand = ReactiveCommand.Create(() => onMove(this, 1));
        }

        public PassFile.Section.Item ToItem() => new()
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
        
        #region Command funcs
        
        private async Task _CopyWhatAsync()
        {
            var what = _NormalizeWhat().Split('\n').FirstOrDefault(x => x != string.Empty) ?? string.Empty;
            await TextCopy.ClipboardService.SetTextAsync(what);

            _dialogService.ShowInfo(string.Format(Resources.STORAGE__ITEM_INFO__WHAT_COPIED, what));
        }

        private async Task _CopyPasswordAsync()
        {
            var password = Password ?? string.Empty;
            await TextCopy.ClipboardService.SetTextAsync(password);
            
            _dialogService.ShowInfo(Resources.STORAGE__ITEM_INFO__PASSWORD_COPIED);
        }

        #endregion
    }
}