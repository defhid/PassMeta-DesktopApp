namespace PassMeta.DesktopApp.Ui.ViewModels.Storage.PassFileWin
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Reactive.Linq;
    using Avalonia.Media;
    using Common;
    using Common.Models.Entities;
    using Common.Utils.Extensions;
    using Components;
    using Constants;
    using ReactiveUI;
    using Utils.Extensions;

    public partial class PassFileWinViewModel : ReactiveObject
    {
        public bool PassFileChanged { get; private set; }
        
        private Action? _closeAction;
        
        public PassFile? PassFile { get; private set; }
        
        public IObservable<string> Title { get; }
        
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

        #endregion

        #region Read-only fields

        public IObservable<string> CreatedOn { get; }
        public IObservable<string> ChangedOn { get; }
        public IObservable<ISolidColorBrush> StateColor { get; }
        public IObservable<string> State { get; }

        #endregion
        
        #region Bottom buttons

        public BtnState OkBtn { get; }
        public BtnState DeleteBtn { get; }
        public BtnState MergeBtn { get; }
        
        #endregion

        public PassFileWinViewModel(PassFile passFile, Action closeAction)
        {
            PassFile = passFile;
            _closeAction = closeAction;

            PassFileChanged = false;
            
            _name = passFile.Name;
            SelectedColorIndex = PassFileColor.List.IndexOf(passFile.GetPassFileColor());

            var passFileChanged = this.WhenAnyValue(vm => vm.PassFile)!;
            
            var passFileNotNew = passFileChanged.Select(pf => pf.Id > 0);
            var anyChanged = this.WhenAnyValue(
                    vm => vm.Name,
                    vm => vm.SelectedColorIndex)
                .Select(val =>
                    val.Item1 != PassFile.Name ||
                    PassFileColor.List[val.Item2] != PassFile.GetPassFileColor());

            Title = passFileChanged.Select(pf => string.Format(pf.Id > 0 
                    ? Resources.PASSFILE__TITLE 
                    : Resources.PASSFILE__TITLE_NEW, pf.Name));
            
            CreatedOn = passFileChanged.Select(pf => pf.CreatedOn == default 
                    ? string.Empty 
                    : pf.CreatedOn.ToString(CultureInfo.CurrentCulture));
            
            ChangedOn = passFileChanged.Select(pf => pf.InfoChangedOn == default 
                    ? string.Empty 
                    : pf.InfoChangedOn.ToString(CultureInfo.CurrentCulture));
            
            StateColor = passFileChanged.Select(pf => pf.GetStateColor());
            
            State = passFileChanged.Select(_MakeState);
            
            
            OkBtn = new BtnState
            {
                ContentObservable = anyChanged.Select(changed => changed ? Resources.PASSFILE__BTN_SAVE : Resources.PASSFILE__BTN_OK),
                CommandObservable = anyChanged.Select(changed => ReactiveCommand.Create(changed ? Save : Close))
            };
            
            DeleteBtn = new BtnState
            {
                CommandObservable = Observable.Return(ReactiveCommand.CreateFromTask(DeleteAsync, passFileNotNew)),
                IsVisibleObservable = Observable.Return(true)
            };
            
            MergeBtn = new BtnState
            {
                CommandObservable = Observable.Return(ReactiveCommand.CreateFromTask(MergeAsync, passFileNotNew)),
                IsVisibleObservable = Observable.Return(true)
            };
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
        public PassFileWinViewModel() {}
#pragma warning restore 8618
    }
}