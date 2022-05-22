namespace PassMeta.DesktopApp.Ui.ViewModels.Storage.PassFileMergeWin.Components
{
    using System;
    using Common.Models.Dto;
    using ReactiveUI;
    
    using ReactCommand = ReactiveUI.ReactiveCommand<System.Reactive.Unit, System.Reactive.Unit>;

    public class ConflictBtn : ReactiveObject
    {
        public readonly PwdMerge.Conflict Conflict;

        public string Name => (Conflict.Local?.Name ?? Conflict.Remote?.Name)!;

        public ReactCommand DeleteCommand { get; }

        public ConflictBtn(PwdMerge.Conflict conflict, Action<ConflictBtn> onDelete)
        {
            Conflict = conflict;
            DeleteCommand = ReactiveCommand.Create(() => onDelete(this));
        }
    }
}