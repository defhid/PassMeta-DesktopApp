namespace PassMeta.DesktopApp.Ui.ViewModels.Storage
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Reactive.Linq;
    using Avalonia.Media;
    using Common;
    using Common.Models.Entities;
    using DynamicData;
    using DynamicData.Binding;
    using Models.Components;
    using Models.Constants;
    using ReactiveUI;
    using Utils.Extensions;
    
    public partial class PassFileWindowViewModel : ReactiveObject
    {
        public event Action<PassFile?>? OnUpdate;
        
        private Action? _close;
        
        private PassFile _passFile;
        public PassFile PassFile
        {
            get => _passFile;
            set => this.RaiseAndSetIfChanged(ref _passFile, value);
        }
        
        private readonly ObservableAsPropertyHelper<string> _title;
        private string Title => _title.Value;
        
        #region Input
        
        private string? _name;
        public string? Name
        {
            get => _name;
            set => this.RaiseAndSetIfChanged(ref _name, value);
        }
        
        private int _selectedColorIndex;
        public int SelectedColorIndex
        {
            get => _selectedColorIndex;
            set => this.RaiseAndSetIfChanged(ref _selectedColorIndex, value);
        }

        private string? _password;
        public string? Password
        {
            get => _password;
            set => this.RaiseAndSetIfChanged(ref _password, value);
        }
        
        private bool _isPasswordBoxVisible;
        public bool IsPasswordBoxVisible
        {
            get => _isPasswordBoxVisible;
            set
            {
                Password = value ? string.Empty : null;
                this.RaiseAndSetIfChanged(ref _isPasswordBoxVisible, value);
            }
        }

        public BtnState PasswordBtn => new(
            this.WhenValueChanged(vm => vm.IsPasswordBoxVisible)
                .Select(isVisible => isVisible ? "\uF78A" : "\uE70F"),
            isVisibleObservable: _passFileNotNew);

        #endregion

        private readonly ObservableAsPropertyHelper<string> _createdOn;
        private string CreatedOn => _createdOn.Value;

        private readonly ObservableAsPropertyHelper<string> _changedOn;
        private string ChangedOn => _changedOn.Value;

        private readonly ObservableAsPropertyHelper<ISolidColorBrush> _stateColor;
        private ISolidColorBrush StateColor => _stateColor.Value;
        
        private readonly ObservableAsPropertyHelper<string> _state;
        private string State => _state.Value;

        #region Bottom buttons

        public BtnState OkBtn => new(
            _anyChanged.Select(changed => changed ? Resources.PASSFILE__BTN_SAVE : Resources.PASSFILE__BTN_OK),
            _anyChanged.Select(changed => ReactiveCommand.Create(changed ? Save : Close)));

        public BtnState DeleteBtn => new(
            commandObservable: Observable.Return(ReactiveCommand.CreateFromTask(DeleteAsync, _passFileNotNew)));

        public BtnState MergeBtn => new(
            commandObservable: Observable.Return(ReactiveCommand.CreateFromTask(MergeAsync, _passFileNotNew)));
        
        #endregion
        
        private readonly ObservableAsPropertyHelper<bool> _isNew;
        private bool IsNew => _isNew.Value;
        
        private readonly IObservable<PassFile> _passFileChanged;
        private readonly IObservable<bool> _passFileNotNew;
        private readonly IObservable<bool> _anyChanged;

        public PassFileWindowViewModel(PassFile passFile, Action close)
        {
            _passFile = passFile;
            _close = close;
            
            _name = passFile.Name;
            _password = null;
            _isPasswordBoxVisible = passFile.Id == 0;
            SelectedColorIndex = PassFileColor.List.IndexOf(passFile.GetPassFileColor());

            _passFileChanged = this.WhenValueChanged(vm => vm.PassFile)!;
            _passFileNotNew = _passFileChanged.Select(pf => pf.Id > 0);
            _anyChanged = this.WhenAnyValue(
                    vm => vm.Name,
                    vm => vm.SelectedColorIndex,
                    vm => vm.Password,
                    vm => vm.IsNew)
                .Select(val =>
                    val.Item1 != _passFile.Name ||
                    PassFileColor.List[val.Item2] != _passFile.GetPassFileColor() ||
                    !string.IsNullOrEmpty(val.Item3) ||
                    val.Item4);

            _title = _passFileChanged.Select(pf => string.Format(pf.Id > 0 
                    ? Resources.PASSFILE__TITLE 
                    : Resources.PASSFILE__TITLE_NEW, pf.Name))
                .ToProperty(this, nameof(Title));
            
            _createdOn = _passFileChanged.Select(pf => pf.CreatedOn == default 
                    ? string.Empty 
                    : pf.CreatedOn.ToString(CultureInfo.CurrentCulture))
                .ToProperty(this, nameof(CreatedOn));
            
            _changedOn = _passFileChanged.Select(pf => pf.InfoChangedOn == default 
                    ? string.Empty 
                    : pf.InfoChangedOn.ToString(CultureInfo.CurrentCulture))
                .ToProperty(this, nameof(ChangedOn));
            
            _stateColor = _passFileChanged.Select(pf => pf.GetStateColor())
                .ToProperty(this, nameof(StateColor));
            
            _state = _passFileChanged.Select(_MakeState)
                .ToProperty(this, nameof(State));

            _isNew = _passFileChanged.Select(pf => pf.Id == 0)
                .ToProperty(this, nameof(IsNew));
        }
        
        private static string _MakeState(PassFile passFile)
        {
            // TODO
            if (passFile.Problem is null && !passFile.LocalCreated && !passFile.LocalChanged && !passFile.LocalDeleted)
            {
                return passFile.Id > 0 ? Resources.PASSFILE__STATE_OK : Resources.PASSFILE__STATE_NEW;
            }
            
            var states = new Stack<string>();

            if (passFile.LocalCreated || passFile.LocalChanged || passFile.LocalDeleted)
                states.Push(string.Format(Resources.PASSFILE__STATE_LOCAL_CHANGED, passFile.InfoChangedOn));

            if (passFile.Problem is not null)
                states.Push(passFile.Problem!.Info);

            return string.Join(",\n", states);
        }
        
#pragma warning disable 8618
        // Avalonia requirements...
        public PassFileWindowViewModel() {}
#pragma warning restore 8618
    }
}