using System;
using PassMeta.DesktopApp.Common.Models.Entities.PassFileMerge;
using ReactiveUI;

namespace PassMeta.DesktopApp.Ui.Models.PassFileMergeWin.Components;

using ReactCommand = ReactiveUI.ReactiveCommand<System.Reactive.Unit, System.Reactive.Unit>;

public class ConflictBtn : ReactiveObject
{
    public readonly PwdPassFileMerge.Conflict Conflict;

    public string Name => (Conflict.Local?.Name ?? Conflict.Remote?.Name)!;

    public ReactCommand DeleteCommand { get; }

    public ConflictBtn(PwdPassFileMerge.Conflict conflict, Action<ConflictBtn> onDelete)
    {
        Conflict = conflict;
        DeleteCommand = ReactiveCommand.Create(() => onDelete(this));
    }
}