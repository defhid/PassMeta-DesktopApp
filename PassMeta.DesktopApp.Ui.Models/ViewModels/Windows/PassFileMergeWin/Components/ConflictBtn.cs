using System;
using System.Reactive;
using PassMeta.DesktopApp.Common.Models.Entities.PassFileMerge;
using ReactiveUI;

namespace PassMeta.DesktopApp.Ui.Models.ViewModels.Windows.PassFileMergeWin.Components;

using ReactCommand = ReactiveCommand<Unit, Unit>;

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