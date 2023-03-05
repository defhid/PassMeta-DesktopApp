using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Linq;
using PassMeta.DesktopApp.Common.Extensions;
using PassMeta.DesktopApp.Common.Models;
using PassMeta.DesktopApp.Common.Models.Entities.PassFile.Data;
using PassMeta.DesktopApp.Common.Models.Entities.PassFileMerge;
using PassMeta.DesktopApp.Ui.Models.ViewModels.Windows.PassFileMergeWin.Components;
using PassMeta.DesktopApp.Ui.Models.ViewModels.Windows.PassFileMergeWin.Extra;
using ReactiveUI;

namespace PassMeta.DesktopApp.Ui.Models.ViewModels.Windows.PassFileMergeWin;

using ReactCommand = ReactiveUI.ReactiveCommand<System.Reactive.Unit, System.Reactive.Unit>;

public class PassFileMergeWinModel : ReactiveObject
{
    public ReactCommand AcceptLocalCommand { get; }
        
    public ReactCommand AcceptRemoteCommand { get; }
        
    public ObservableCollection<ConflictBtn> ConflictButtons { get; }

    private ConflictBtn? _selectedConflictBtn;
    public ConflictBtn? SelectedConflictBtn
    {
        get => _selectedConflictBtn;
        set => this.RaiseAndSetIfChanged(ref _selectedConflictBtn, value);
    }

    private readonly ObservableAsPropertyHelper<ObservableCollection<ItemBtn>> _localItems;
    public ObservableCollection<ItemBtn> LocalItems => _localItems.Value;
        
    private readonly ObservableAsPropertyHelper<ObservableCollection<ItemBtn>> _remoteItems;
    public ObservableCollection<ItemBtn> RemoteItems => _remoteItems.Value;
        
    public IObservable<string> LocalSectionName { get; }
    public IObservable<string> RemoteSectionName { get; }
        
    public string LocalVersion => $"v{_passFileMerge.Versions.Branching}..{_passFileMerge.Versions.Local}";
    public string RemoteVersion => $"v{_passFileMerge.Versions.Branching}..{_passFileMerge.Versions.Remote}";

    public string LocalVersionDate => _passFileMerge.VersionsChangedOn.Local.ToShortDateTimeString();
    public string RemoteVersionDate => _passFileMerge.VersionsChangedOn.Remote.ToShortDateTimeString();

    public readonly ViewElements ViewElements = new();

    private readonly PwdPassFileMerge _passFileMerge;

    public PassFileMergeWinModel(PwdPassFileMerge passFileMerge)
    {
        _passFileMerge = passFileMerge;
        ConflictButtons = new ObservableCollection<ConflictBtn>(passFileMerge.Conflicts.Select(_MakeConflictBtn));
            
        var conflictChanged = this.WhenAnyValue(vm => vm.SelectedConflictBtn);
            
        AcceptLocalCommand = ReactiveCommand.Create(() => _Accept(true), conflictChanged.Select(btn => btn is not null));
        AcceptRemoteCommand = ReactiveCommand.Create(() => _Accept(false), conflictChanged.Select(btn => btn is not null));

        _localItems = conflictChanged.Select(btn => new ObservableCollection<ItemBtn>(
                btn?.Conflict.Local?.Items.Select(_MakeItemBtn) ?? Array.Empty<ItemBtn>()))
            .ToProperty(this, nameof(LocalItems));
            
        _remoteItems = conflictChanged.Select(btn => new ObservableCollection<ItemBtn>(
                btn?.Conflict.Remote?.Items.Select(_MakeItemBtn) ?? Array.Empty<ItemBtn>()))
            .ToProperty(this, nameof(RemoteItems));

        LocalSectionName = conflictChanged.Select(btn => btn?.Conflict.Local?.Name ?? string.Empty);
        RemoteSectionName = conflictChanged.Select(btn => btn?.Conflict.Remote?.Name ?? string.Empty);
    }

    private void Close() => ViewElements.Window!.Close(Result.From(!_passFileMerge.Conflicts.Any()));

    private void _Accept(bool isLocal)
    {
        var items = isLocal ? LocalItems : RemoteItems;
        var conflictBtn = SelectedConflictBtn!;
        var conflict = conflictBtn.Conflict;
            
        _passFileMerge.Result.Add(new PwdSection
        {
            Id = (conflict.Local?.Id ?? conflict.Remote?.Id)!.Value,
            Name = (conflict.Local?.Name ?? conflict.Remote?.Name)!,
            Items = items.Select(btn => btn.ToItem()).ToList()
        });
        _passFileMerge.Conflicts.Remove(conflict);

        var index = ConflictButtons.IndexOf(conflictBtn);
            
        ConflictButtons.Remove(conflictBtn);
        if (!ConflictButtons.Any())
        {
            Close();
        }
        else
        {
            SelectedConflictBtn = index < ConflictButtons.Count
                ? ConflictButtons[index]
                : ConflictButtons[index - 1];
        }
    }

    #region Buttons factory

    private ConflictBtn _MakeConflictBtn(PwdPassFileMerge.Conflict conflict)
        => new(conflict, btn =>
        {
            _passFileMerge.Conflicts.Remove(btn.Conflict);
            ConflictButtons.Remove(btn);
            if (!ConflictButtons.Any())
            {
                Close();
            }
        });

    private ItemBtn _MakeItemBtn(PwdItem item)
        => new(item, (btn) =>
        {
            LocalItems.Remove(btn);
            RemoteItems.Remove(btn);
        }, (btn, direction) =>
        {
            void Move(ObservableCollection<ItemBtn> list)
            {
                var index = list.IndexOf(btn);
                if (index < 0) return;

                if (direction > 0 && index + direction < list.Count
                    || direction < 0 && index + direction > -1)
                {
                    list.Move(index, index + direction);
                }
            }
                
            Move(LocalItems);
            Move(RemoteItems);
        }, (btn) =>
        {
            if (LocalItems.Remove(btn))
            {
                RemoteItems.Add(btn);
            }
            else
            {
                RemoteItems.Remove(btn);
                LocalItems.Add(btn);
            }
        });

    #endregion
        
#pragma warning disable 8618
    public PassFileMergeWinModel() {}
#pragma warning restore 8618
}