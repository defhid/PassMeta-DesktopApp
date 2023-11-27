using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading.Tasks;
using Avalonia.Media;
using PassMeta.DesktopApp.Common;
using PassMeta.DesktopApp.Common.Enums;
using PassMeta.DesktopApp.Common.Extensions;
using PassMeta.DesktopApp.Common.Mapping.Values;
using PassMeta.DesktopApp.Common.Models.Entities.PassFile;
using PassMeta.DesktopApp.Ui.Models.Constants;
using PassMeta.DesktopApp.Ui.Models.Extensions;
using PassMeta.DesktopApp.Ui.Models.ViewModels.Common;
using ReactiveUI;

namespace PassMeta.DesktopApp.Ui.Models.ViewModels.Windows.PassFileWin;

public abstract class PassFileWinModel : ReactiveObject
{
    private string? _name;
    private int _selectedColorIndex;

    protected readonly BehaviorSubject<PassFile> ChangedSource;
    public readonly Interaction<Unit, Action> Quit = new();
    public readonly PassFile PassFile;

    protected PassFileWinModel(PassFile passFile)
    {
        PassFile = passFile;
        ChangedSource = new BehaviorSubject<PassFile>(passFile);

        _name = passFile.Name;
        _selectedColorIndex = PassFileColor.List.IndexOf(passFile.GetPassFileColor());

        Title = ChangedSource.Select(pf => string.Format(pf.IsLocalCreated()
            ? Resources.PASSFILE__TITLE_NEW
            : pf.IsLocalDeleted()
                ? Resources.PASSFILE__TITLE_DELETED
                : Resources.PASSFILE__TITLE, pf.GetIdentityString()));

        CreatedOn = ChangedSource.Select(pf => pf.CreatedOn.ToLocalTime().ToShortDateTimeString());

        ChangedOn = ChangedSource.Select(pf =>
            new[] { pf.InfoChangedOn, pf.VersionChangedOn }.Max().ToLocalTime().ToShortDateTimeString());

        StateColor = ChangedSource.Select(pf => pf.GetStateColor());

        State = ChangedSource.Select(MakeState);

        ReadOnly = ChangedSource.Select(pf => pf.IsLocalDeleted());

        var anyChanged = this.WhenAnyValue(
                vm => vm.Name,
                vm => vm.SelectedColorIndex)
            .CombineLatest(ChangedSource, (first, second) => 
                first.Item1 != second.Name || 
                PassFileColor.List[first.Item2] != second.GetPassFileColor());

        OkBtn = new BtnState
        {
            ContentObservable = anyChanged.Select(changed => changed
                ? Resources.PASSFILE__BTN_SAVE
                : Resources.PASSFILE__BTN_OK),
            CommandObservable = anyChanged.Select(changed => changed
                ? ReactiveCommand.Create(Save)
                : ReactiveCommand.CreateFromTask(QuitAsync))
        };

        ChangePasswordBtn = new BtnState
        {
            CommandObservable = Observable.Return(ReactiveCommand.CreateFromTask(ChangePasswordAsync)),
            IsVisibleObservable = ChangedSource.Select(pf => !pf.IsLocalDeleted())
        };

        MergeBtn = new BtnState
        {
            CommandObservable = Observable.Return(ReactiveCommand.CreateFromTask(MergeAsync)),
            IsVisibleObservable = ChangedSource.Select(pf =>
                !pf.IsLocalDeleted() &&
                pf.Mark.HasFlag(PassFileMark.NeedsMerge))
        };

        ExportBtn = new BtnState
        {
            CommandObservable = Observable.Return(ReactiveCommand.CreateFromTask(ExportAsync)),
            IsVisibleObservable = ChangedSource.Select(pf => !pf.IsLocalDeleted())
        };

        RestoreBtn = new BtnState
        {
            CommandObservable = Observable.Return(ReactiveCommand.CreateFromTask(RestoreAsync))
        };

        DeleteBtn = new BtnState
        {
            CommandObservable = Observable.Return(ReactiveCommand.CreateFromTask(DeleteAsync)),
            IsVisibleObservable = ChangedSource.Select(pf => !pf.IsLocalDeleted())
        };
    }
    
    public IObservable<string> Title { get; }
    public IObservable<bool> ReadOnly { get; }
    public IObservable<string?> CreatedOn { get; }
    public IObservable<string?> ChangedOn { get; }
    public IObservable<ISolidColorBrush> StateColor { get; }
    public IObservable<string> State { get; }
    
    public BtnState OkBtn { get; }
    public BtnState ChangePasswordBtn { get; }
    public BtnState MergeBtn { get; }
    public BtnState ExportBtn { get; }
    public BtnState RestoreBtn { get; }
    public BtnState DeleteBtn { get; }

    public string? Name
    {
        get => _name;
        set => this.RaiseAndSetIfChanged(ref _name, value);
    }

    public int SelectedColorIndex
    {
        get => _selectedColorIndex;
        set => this.RaiseAndSetIfChanged(ref _selectedColorIndex, value);
    }

    protected async Task QuitAsync()
    {
        var action = await Quit.Handle(default);
        action();
    }

    protected abstract void Save();

    protected abstract Task ChangePasswordAsync();

    protected abstract Task ExportAsync();

    protected abstract Task RestoreAsync();

    protected abstract Task MergeAsync();

    protected abstract Task DeleteAsync();
    
    private static string MakeState(PassFile passFile)
    {
        var states = new Stack<string>();

        states.Push(string.Format(
            passFile.IsLocalCreated()
                ? Resources.PASSFILE__STATE_LOCAL_CREATED
                : passFile.IsLocalChanged()
                    ? Resources.PASSFILE__STATE_LOCAL_CHANGED
                    : passFile.IsLocalDeleted()
                        ? Resources.PASSFILE__STATE_LOCAL_DELETED
                        : passFile.Mark == PassFileMark.None
                            ? Resources.PASSFILE__STATE_OK
                            : string.Empty, passFile.GetPassFileChangePeriod()));

        if (passFile.Mark != PassFileMark.None)
            states.Push(PassFileMarkMapping.MarkToName.Map(passFile.Mark, "?"));

        return string.Join(Environment.NewLine, states.Where(s => s != string.Empty));
    }
}