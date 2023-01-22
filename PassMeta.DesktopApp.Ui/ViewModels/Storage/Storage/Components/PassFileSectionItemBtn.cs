using PassMeta.DesktopApp.Common.Models.Entities.PassFile.Data;

namespace PassMeta.DesktopApp.Ui.ViewModels.Storage.Storage.Components
{
    using System;
    using System.Linq;
    using System.Reactive.Linq;
    using System.Reactive.Subjects;
    using System.Threading.Tasks;
    using Common;
    using Common.Abstractions.Services;
    using Core;
    using ReactiveUI;
    
    using ReactCommand = ReactiveUI.ReactiveCommand<System.Reactive.Unit, System.Reactive.Unit>;

    public class PassFileSectionItemBtn : ReactiveObject
    {
        private readonly IDialogService _dialogService = EnvironmentContainer.Resolve<IDialogService>();
        private readonly IClipboardService _clipboardService = EnvironmentContainer.Resolve<IClipboardService>();
        
        private readonly ObservableAsPropertyHelper<bool> _isReadOnly;
        public bool IsReadOnly => _isReadOnly.Value;
        
        public string? Comment { get; set; }
        public string? What { get; set; }

        private string? _password;
        public string? Password
        {
            get => _password;
            set => this.RaiseAndSetIfChanged(ref _password, value);
        }

        public IObservable<bool> IsCommentTextVisible { get; }
        public IObservable<bool> IsCommentInputVisible { get; }
        public IObservable<char?> PasswordChar { get; }
        public IObservable<bool> PopupGeneratorCanBeOpened { get; }

        public ReactCommand CopyWhatCommand { get; }
        public ReactCommand CopyPasswordCommand { get; }
        public ReactCommand DeleteCommand { get; }
        public ReactCommand UpCommand { get; }
        public ReactCommand DownCommand { get; }
        public ReactCommand OpenPopupGenerator { get; }

        private readonly BehaviorSubject<bool> _isPopupGeneratorOpened = new(false);
        public PopupGeneratorViewModel PopupGenerator { get; }

        public PassFileSectionItemBtn(PwdItem item,
            IObservable<bool> editModeObservable,
            Action<PassFileSectionItemBtn> onDelete,
            Action<PassFileSectionItemBtn, int> onMove)
        {
            What = string.Join('\n', item.Usernames.Select(x => x.Trim()).Where(x => x != string.Empty));
            Password = item.Password;
            Comment = item.Remark;

            _isReadOnly = editModeObservable.Select(editMode => !editMode)
                .ToProperty(this, nameof(IsReadOnly));
            
            IsCommentTextVisible = this.WhenAnyValue(btn => btn.IsReadOnly, btn => btn.Comment)
                .Select(pair => pair.Item1 && pair.Item2 != string.Empty);
            
            IsCommentInputVisible = this.WhenAnyValue(btn => btn.IsReadOnly)
                .Select(isReadOnly => !isReadOnly);

            PasswordChar = this.WhenAnyValue(btn => btn.IsReadOnly)
                .Select(isReadOnly => isReadOnly && AppConfig.Current.HidePasswords ? '*' : (char?)null);

            PopupGeneratorCanBeOpened = this.WhenAnyValue(
                    btn => btn.IsReadOnly,
                    btn => btn.Password)
                .Select(x => !x.Item1 && string.IsNullOrEmpty(x.Item2));

            CopyWhatCommand = ReactiveCommand.CreateFromTask(_CopyWhatAsync);
            CopyPasswordCommand = ReactiveCommand.CreateFromTask(_CopyPasswordAsync);
            
            DeleteCommand = ReactiveCommand.Create(() => onDelete(this));
            UpCommand = ReactiveCommand.Create(() => onMove(this, -1));
            DownCommand = ReactiveCommand.Create(() => onMove(this, 1));

            OpenPopupGenerator = ReactiveCommand.Create(() =>
            {
                _isPopupGeneratorOpened.OnNext(false);
                _isPopupGeneratorOpened.OnNext(true);
            });

            PopupGenerator = new PopupGeneratorViewModel(_isPopupGeneratorOpened, pwd => Password = pwd);
        }

        public PwdItem ToItem() => new()
        {
            Usernames = _NormalizeWhat().Split('\n'),
            Password = Password ?? string.Empty,
            Remark = Comment?.Trim() ?? string.Empty
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

            if (await _clipboardService.TrySetTextAsync(what))
            {
                _dialogService.ShowInfo(string.Format(Resources.STORAGE__ITEM_INFO__WHAT_COPIED, what));
            }
        }

        private async Task _CopyPasswordAsync()
        {
            var password = Password ?? string.Empty;
            
            if (await _clipboardService.TrySetTextAsync(password))
            {
                _dialogService.ShowInfo(Resources.STORAGE__ITEM_INFO__PASSWORD_COPIED);
            }
        }

        #endregion
    }
}