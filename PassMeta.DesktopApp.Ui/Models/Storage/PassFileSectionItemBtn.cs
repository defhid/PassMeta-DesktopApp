namespace PassMeta.DesktopApp.Ui.Models.Storage
{
    using System;
    using DesktopApp.Common.Models.Entities;
    using System.Linq;
    using System.Reactive.Linq;
    using System.Threading.Tasks;
    using Common;
    using Common.Interfaces.Services;
    using DynamicData.Binding;
    using ReactiveUI;
    using Splat;

    public class PassFileSectionItemBtn : ReactiveObject
    {
        public string? What { get; set; }

        public string? Password { get; set; }

        private string? _comment;
        public string? Comment
        {
            get => _comment;
            set => this.RaiseAndSetIfChanged(ref _comment, value);
        }

        private bool _isReadOnly;
        public bool IsReadOnly
        {
            get => _isReadOnly;
            set => this.RaiseAndSetIfChanged(ref _isReadOnly, value);
        }
        
        private readonly ObservableAsPropertyHelper<bool> _isCommentTextVisible;
        public bool IsCommentTextVisible => _isCommentTextVisible.Value;

        private readonly ObservableAsPropertyHelper<bool> _isCommentInputVisible;
        public bool IsCommentInputVisible => _isCommentInputVisible.Value;

        private readonly Action<PassFileSectionItemBtn> _onDelete;

        private readonly  Action<PassFileSectionItemBtn, int> _onMove;

        public PassFileSectionItemBtn(PassFile.Section.Item item,
            bool readOnly,
            Action<PassFileSectionItemBtn> onDelete,
            Action<PassFileSectionItemBtn, int> onMove)
        {
            What = string.Join('\n', item.What.Select(x => x.Trim()).Where(x => x != string.Empty));
            Password = item.Password;
            Comment = item.Comment;
            IsReadOnly = readOnly;

            _onDelete = onDelete;
            _onMove = onMove;

            _isCommentTextVisible = this.WhenAnyValue(btn => btn.IsReadOnly, btn => btn.Comment)
                .Select(pair => pair.Item1 && pair.Item2 != string.Empty)
                .ToProperty(this, btn => btn.IsCommentTextVisible);
            
            _isCommentInputVisible = this.WhenValueChanged(btn => btn.IsReadOnly)
                .Select(isReadOnly => !isReadOnly)
                .ToProperty(this, btn => btn.IsCommentInputVisible);
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
        
        #region Commands
        
        private async Task CopyWhatAsyncCommand()
        {
            var what = _NormalizeWhat().Split('\n').FirstOrDefault(x => x != string.Empty) ?? string.Empty;
            await TextCopy.ClipboardService.SetTextAsync(what);

            await Locator.Current.GetService<IDialogService>()!
                .ShowInfoAsync(string.Format(Resources.STORAGE__WHAT_COPIED, what));
        }

        private async Task CopyPasswordAsyncCommand()
        {
            var password = Password ?? string.Empty;
            await TextCopy.ClipboardService.SetTextAsync(password);
            
            await Locator.Current.GetService<IDialogService>()!
                .ShowInfoAsync(string.Format(Resources.STORAGE__PASSWORD_COPIED, password));
        }

        private void DeleteCommand() => _onDelete(this);
        
        private void UpCommand() => _onMove(this, -1);
        
        private void DownCommand() => _onMove(this, 1);

        #endregion
    }
}