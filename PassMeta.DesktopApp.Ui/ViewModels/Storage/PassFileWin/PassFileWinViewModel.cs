namespace PassMeta.DesktopApp.Ui.ViewModels.Storage.PassFileWin
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reactive.Linq;
    using Avalonia.Media;
    using Common;
    using Common.Enums;
    using Common.Models.Entities;
    using Common.Utils.Extensions;
    using Components;
    using Constants;
    using Models;
    using ReactiveUI;
    using Utils.Extensions;

    public partial class PassFileWinViewModel : ReactiveObject
    {
        public bool PassFileChanged { get; private set; }

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

        public IObservable<string?> CreatedOn { get; }
        public IObservable<string?> ChangedOn { get; }
        public IObservable<ISolidColorBrush> StateColor { get; }
        public IObservable<string> State { get; }

        #endregion
        
        #region Bottom buttons

        public BtnState OkBtn { get; }
        public BtnState ChangePasswordBtn { get; }
        public BtnState MergeBtn { get; }
        public BtnState DeleteBtn { get; }
        
        #endregion
        
        public IObservable<bool> ReadOnly { get; }

        public ViewElements ViewElements { get; } = new();

        public PassFileWinViewModel(PassFile passFile)
        {
            PassFile = passFile;

            PassFileChanged = false;
            
            _name = passFile.Name;
            SelectedColorIndex = PassFileColor.List.IndexOf(passFile.GetPassFileColor());

            var passFileChanged = this.WhenAnyValue(vm => vm.PassFile);
            
            var anyChanged = this.WhenAnyValue(
                    vm => vm.Name,
                    vm => vm.SelectedColorIndex,
                    vm => vm.PassFile)
                .Select(val =>
                    val.Item3 is not null && (
                        val.Item1 != val.Item3.Name || 
                        PassFileColor.List[val.Item2] != val.Item3.GetPassFileColor()));

            Title = passFileChanged.Select(pf => pf is null
                ? string.Empty
                : string.Format(pf.LocalCreated
                    ? Resources.PASSFILE__TITLE_NEW
                    : pf.LocalDeleted
                        ? Resources.PASSFILE__TITLE_DELETED
                        : Resources.PASSFILE__TITLE, pf.Name, pf.Id));
            
            CreatedOn = passFileChanged.Select(pf => pf?.CreatedOn.ToShortDateTimeString());
            
            ChangedOn = passFileChanged.Select(pf => pf?.InfoChangedOn.ToShortDateTimeString());
            
            StateColor = passFileChanged.Select(pf => pf.GetStateColor());
            
            State = passFileChanged.Select(_MakeState);

            OkBtn = new BtnState
            {
                ContentObservable = anyChanged.Select(changed => changed 
                    ? Resources.PASSFILE__BTN_SAVE
                    : Resources.PASSFILE__BTN_OK),
                CommandObservable = anyChanged.Select(changed => ReactiveCommand.Create(changed ? Save : Close))
            };

            ChangePasswordBtn = new BtnState
            {
                CommandObservable = Observable.Return(ReactiveCommand.CreateFromTask(ChangePasswordAsync)),
                IsVisibleObservable = passFileChanged.Select(pf => pf?.LocalDeleted is false)
            };
            
            MergeBtn = new BtnState
            {
                CommandObservable = Observable.Return(ReactiveCommand.CreateFromTask(MergeAsync)),
                IsVisibleObservable = passFileChanged.Select(pf => pf?.Problem?.Kind is PassFileProblemKind.NeedsMerge)
            };
            
            DeleteBtn = new BtnState
            {
                ContentObservable = passFileChanged.Select(pf => pf?.LocalDeleted is false 
                    ? Resources.PASSFILE__BTN_DELETE
                    : Resources.PASSFILE__BTN_RESTORE),
                CommandObservable = passFileChanged.Select(pf => ReactiveCommand.CreateFromTask(pf?.LocalDeleted is false 
                    ? DeleteAsync
                    : RestoreAsync)),
                IsVisibleObservable = passFileChanged.Select(pf => pf is not null)
            };

            ReadOnly = passFileChanged.Select(pf => pf?.LocalDeleted is not false);
        }
        
        private static string _MakeState(PassFile? passFile)
        {
            if (passFile is null) return string.Empty;

            var states = new Stack<string>();
            states.Push(string.Format(
                passFile.LocalCreated
                    ? Resources.PASSFILE__STATE_LOCAL_CREATED
                    : passFile.LocalChanged
                        ? Resources.PASSFILE__STATE_LOCAL_CHANGED
                        : passFile.LocalDeleted
                            ? Resources.PASSFILE__STATE_LOCAL_DELETED
                            : passFile.Problem is null
                                ? Resources.PASSFILE__STATE_OK
                                : string.Empty, passFile.GetPassFileChangePeriod()));
            
            if (passFile.Problem is not null)
                states.Push(passFile.Problem.ToString());

            return string.Join(Environment.NewLine, states.Where(s => s != string.Empty));
        }

        #region Avalonia requirements...
#pragma warning disable 8618
        public PassFileWinViewModel() {}
#pragma warning restore 8618
        #endregion
    }
}