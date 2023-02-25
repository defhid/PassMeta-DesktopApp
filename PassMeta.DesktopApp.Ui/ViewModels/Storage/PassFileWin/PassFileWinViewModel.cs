using PassMeta.DesktopApp.Common.Extensions;
using PassMeta.DesktopApp.Common.Models.Entities.PassFile;
using PassMeta.DesktopApp.Core.Extensions;
using PassMeta.DesktopApp.Ui.Extensions;
using PassMeta.DesktopApp.Ui.Interfaces.Services;
using Splat;

namespace PassMeta.DesktopApp.Ui.ViewModels.Storage.PassFileWin;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Avalonia.Media;
using Common;
using Common.Abstractions.Services;
using Components;
using Constants;
using Core;
using Core.Utils;
using Models;
using ReactiveUI;

public class PassFileWinViewModel : ReactiveObject
{
    public bool PassFileChanged { get; private set; }

    private PassFile? _passFile;
    public PassFile? PassFile 
    { 
        get => _passFile;
        private set => this.RaiseAndSetIfChanged(ref _passFile, value);
    }
        
    public IObservable<string> Title { get; }
        
    public IObservable<bool> ReadOnly { get; }
        
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
    public BtnState ExportBtn { get; }
    public BtnState RestoreBtn { get; }
    public BtnState DeleteBtn { get; }
        
    #endregion
        
    public readonly ViewElements ViewElements = new();

    private readonly IDialogService _dialogService = Locator.Current.Resolve<IDialogService>();

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
                    : Resources.PASSFILE__TITLE, pf.GetIdentityString()));

        CreatedOn = passFileChanged.Select(pf => pf?.CreatedOn.ToLocalTime().ToShortDateTimeString());

        ChangedOn = passFileChanged.Select(pf => string.Join(" / ", 
            new[] {pf?.InfoChangedOn, pf?.VersionChangedOn}
                .Where(dt => dt is not null)
                .Select(dt => dt!.Value.ToLocalTime().ToShortDateTimeString())));

        StateColor = passFileChanged.Select(pf => pf.GetStateColor());

        State = passFileChanged.Select(pf => pf is null ? string.Empty : _MakeState(pf));

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
            IsVisibleObservable = passFileChanged.Select(pf => pf?.LocalDeleted is false
                                                               && pf.Problem?.Kind is PassFileProblemKind.NeedsMerge)
        };
            
        ExportBtn = new BtnState
        {
            CommandObservable = Observable.Return(ReactiveCommand.CreateFromTask(ExportAsync)),
            IsVisibleObservable = passFileChanged.Select(pf => pf?.LocalDeleted is false)
        };

        RestoreBtn = new BtnState
        {
            CommandObservable = Observable.Return(ReactiveCommand.CreateFromTask(RestoreAsync)),
            IsVisibleObservable = passFileChanged.Select(pf => pf is not null)
        };

        DeleteBtn = new BtnState
        {
            CommandObservable = Observable.Return(ReactiveCommand.Create(Delete)),
            IsVisibleObservable = passFileChanged.Select(pf => pf?.LocalDeleted is false)
        };

        ReadOnly = passFileChanged.Select(pf => pf?.LocalDeleted is not false);
    }
        
    private static string _MakeState(PassFile passFile)
    {
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

    public void Close() => ViewElements.Window!.Close();

    public async Task ChangePasswordAsync()
    {
        if (PassFile?.LocalDeleted is not false) return;

        var result = await PassFile.LoadIfRequiredAndDecryptAsync(_dialogService, true);
        if (result.Bad) return;

        var passPhraseNew = await _dialogService.AskPasswordAsync(Resources.PASSFILE__ASK_PASSPHRASE_NEW);
        if (passPhraseNew.Bad || passPhraseNew.Data == string.Empty) return;

        var passfile = PassFile.Copy();
        passfile.PassPhrase = passPhraseNew.Data;
        var updateResult = PassFileManager.UpdateData(passfile);

        if (updateResult.Ok)
        {
            PassFile = passfile;
            PassFileChanged = true;
                
            _dialogService.ShowInfo(Resources.PASSFILE__INFO_PASSPHRASE_CHANGED);
        }
        else
        {
            _dialogService.ShowError(updateResult.Message!);
        }
    }
        
    private void Save()
    {
        if (PassFile is null)
        {
            Close();
            return;
        }

        if (PassFile.LocalDeleted)
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(Name))
        {
            _dialogService.ShowFailure(Resources.PASSFILE__VALIDATION__INCORRECT_NAME);
            return;
        }

        var passFile = PassFile.Copy();
        passFile.Name = Name.Trim();
        passFile.Color = PassFileColor.List[SelectedColorIndex].Hex;

        var result = PassFileManager.UpdateInfo(passFile);
        if (result.Ok)
        {
            PassFile = passFile;
            PassFileChanged = true;
        }
        else
        {
            _dialogService.ShowError(result.Message!);
        }
    }

    private void Delete()
    {
        if (PassFile?.LocalDeleted is not false) return;

        PassFile = PassFileManager.Delete(PassFile);
        PassFileChanged = true;

        if (PassFile is null) Close();
    }

    private Task ExportAsync()
    {
        if (PassFile?.LocalDeleted is not false)
        {
            return Task.CompletedTask;
        }

        var exportService = Locator.Current.Resolve<IPassFileExportUiService>();

        return exportService.SelectAndExportAsync(PassFile, ViewElements.Window!);
    }

    private async Task RestoreAsync()
    {
        var passFile = PassFile;
        if (passFile is null) return;

        var restoreService = Locator.Current.Resolve<IPassFileRestoreUiService>();

        var result = await restoreService.SelectAndRestoreAsync(passFile, ViewElements.Window!);
        if (result.Bad) return;
            
        PassFile = null;
        PassFile = passFile;
        PassFileChanged = true;
    }
        
    private async Task MergeAsync()
    {
        var passFile = PassFile;
        if (passFile?.LocalDeleted is not false) return;
            
        var mergeService = Locator.Current.Resolve<IPassFileMergeUiService>();

        var result = await mergeService.LoadRemoteAndMergeAsync(passFile, ViewElements.Window!);
        if (result.Bad) return;

        PassFile = null;
        PassFile = passFile;
        PassFileChanged = true;
    }

#pragma warning disable 8618
    public PassFileWinViewModel() {}
#pragma warning restore 8618
}