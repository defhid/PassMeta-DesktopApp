namespace PassMeta.DesktopApp.Ui.ViewModels.Storage
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Reactive.Linq;
    using System.Windows.Input;
    using Avalonia.Media;
    using Common;
    using Common.Models.Entities;
    using DynamicData;
    using DynamicData.Binding;
    using Models.PassFile;
    using ReactiveUI;
    using Utils.Extensions;
    
    public partial class PassFileViewModel : ReactiveObject
    {
        private Action<PassFile?>? _close;
        
        private PassFile _passFile;
        public PassFile PassFile
        {
            get => _passFile;
            set => this.RaiseAndSetIfChanged(ref _passFile, value);
        }
        
        private readonly ObservableAsPropertyHelper<string> _title;
        private string Title => _title.Value;
        
        #region Input
        
        private string _name = string.Empty;
        public string Name
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
                Password = null;
                this.RaiseAndSetIfChanged(ref _isPasswordBoxVisible, value);
            }
        }
        
        private readonly ObservableAsPropertyHelper<string> _passwordBtnContent;
        public string PasswordBtnContent => _passwordBtnContent.Value;
        
        #endregion
        
        private readonly ObservableAsPropertyHelper<string> _createdOn;
        private string CreatedOn => _createdOn.Value;
        
        private readonly ObservableAsPropertyHelper<string> _changedOn;
        private string ChangedOn => _changedOn.Value;

        private readonly ObservableAsPropertyHelper<ISolidColorBrush?> _stateColor;
        private ISolidColorBrush? StateColor => _stateColor.Value;
        
        private readonly ObservableAsPropertyHelper<string> _state;
        private string State => _state.Value;

        #region Buttons
        
        private readonly ObservableAsPropertyHelper<string> _okBtnContent;
        public string OkBtnContent => _okBtnContent.Value;
        
        private readonly ObservableAsPropertyHelper<ICommand> _okBtnCommand;
        public ICommand OkBtnCommand => _okBtnCommand.Value;
        
        private readonly ObservableAsPropertyHelper<string> _archiveBtnContent;
        public string ArchiveBtnContent => _archiveBtnContent.Value;
        
        private readonly ObservableAsPropertyHelper<ICommand> _archiveBtnCommand;
        public ICommand ArchiveBtnCommand => _archiveBtnCommand.Value;
        
        #endregion
        
        private readonly ObservableAsPropertyHelper<bool> _anyChanged;
        private bool AnyChanged => _anyChanged.Value;

        public PassFileViewModel(PassFile passFile, Action<PassFile?> close)
        {
            _passFile = passFile;
            _close = close;
            
            _name = passFile.Name;
            _password = null;
            SelectedColorIndex = PassFileColor.List.IndexOf(passFile.GetPassFileColor());

            var passFileChanged = this.WhenValueChanged(vm => vm.PassFile);

            _title = passFileChanged.Select(pf => string.Format(Resources.STORAGE_PASSFILE__TITLE, pf!.Name))
                .ToProperty(this, vm => vm.Title);
            
            _createdOn = passFileChanged.Select(pf => pf!.CreatedOn.ToString(CultureInfo.CurrentCulture))
                .ToProperty(this, vm => vm.CreatedOn);
            
            _changedOn = passFileChanged.Select(pf => pf!.ChangedOn.ToString(CultureInfo.CurrentCulture))
                .ToProperty(this, vm => vm.ChangedOn);
            
            _stateColor = passFileChanged.Select(pf => pf!.GetStateColor())
                .ToProperty(this, vm => vm.StateColor);
            
            _state = passFileChanged.Select(pf => _MakeState(pf!))
                .ToProperty(this, vm => vm.State);

            _passwordBtnContent = this.WhenValueChanged(vm => vm.IsPasswordBoxVisible)
                .Select(isVisible => isVisible ? "\uF78A" : "\uE70F")
                .ToProperty(this, vm => vm.PasswordBtnContent);

            _anyChanged = this.WhenAnyValue(
                    vm => vm.Name,
                    vm => vm.SelectedColorIndex,
                    vm => vm.Password)
                .Select(val =>
                    val.Item1 != _passFile.Name ||
                    PassFileColor.List[val.Item2] != _passFile.GetPassFileColor() ||
                    !string.IsNullOrEmpty(val.Item3))
                .ToProperty(this, vm => vm.AnyChanged);
            
            var anyChanged = this.WhenValueChanged(vm => vm.AnyChanged);

            _okBtnContent = anyChanged.Select(changed => changed 
                    ? Resources.PASSFILE__BTN_SAVE 
                    : Resources.PASSFILE__BTN_OK)
                .ToProperty(this, vm => vm.OkBtnContent);
            
            _okBtnCommand = anyChanged.Select(changed => changed 
                    ? ReactiveCommand.Create(Save) 
                    : ReactiveCommand.Create(Close))
                .ToProperty(this, vm => vm.OkBtnCommand);
            
            _archiveBtnContent = passFileChanged
                .Select(pf => pf!.IsArchived 
                    ? Resources.PASSFILE__BTN_UNARCHIVE 
                    : Resources.PASSFILE__BTN_ARCHIVE)
                .ToProperty(this, vm => vm.ArchiveBtnContent);
            
            _archiveBtnCommand = passFileChanged
                .Select(pf => pf!.IsArchived
                    ? ReactiveCommand.Create(UnArchive)
                    : ReactiveCommand.Create(Archive))
                .ToProperty(this, vm => vm.ArchiveBtnCommand);
        }
        
        private static string _MakeState(PassFile passFile)
        {
            if (!passFile.HasProblem && !passFile.IsLocalChanged)
            {
                return Resources.PASSFILE__STATE_OK;
            }
            
            var states = new Stack<string>();

            if (passFile.IsLocalChanged)
                states.Push(string.Format(Resources.PASSFILE__STATE_LOCAL_CHANGED, passFile.ChangedLocalOn!.Value));

            if (passFile.HasProblem)
                states.Push(passFile.Problem!.Info);

            return string.Join(",\n", states);
        }
        
#pragma warning disable 8618
        // Avalonia requirements...
        public PassFileViewModel() {}
#pragma warning restore 8618
    }
}