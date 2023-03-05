using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Avalonia.Media;
using PassMeta.DesktopApp.Common;
using PassMeta.DesktopApp.Common.Abstractions.PassFileContext;
using PassMeta.DesktopApp.Common.Abstractions.Services;
using PassMeta.DesktopApp.Common.Abstractions.Utils.Helpers;
using PassMeta.DesktopApp.Common.Enums;
using PassMeta.DesktopApp.Common.Extensions;
using PassMeta.DesktopApp.Common.Mapping.Values;
using PassMeta.DesktopApp.Common.Models.Entities.PassFile;
using PassMeta.DesktopApp.Common.Models.Entities.PassFile.Data;
using PassMeta.DesktopApp.Ui.Models.Abstractions.Services;
using PassMeta.DesktopApp.Ui.Models.Common.Components;
using PassMeta.DesktopApp.Ui.Models.Common.Constants;
using PassMeta.DesktopApp.Ui.Models.Common.Extensions;
using PassMeta.DesktopApp.Ui.Models.PassFileWin.Models;
using ReactiveUI;
using Splat;

namespace PassMeta.DesktopApp.Ui.Models.PassFileWin;

public class PassFileWinViewModel : ReactiveObject
{
    private PwdPassFile? _passFile;

    public PwdPassFile? PassFile
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
    private readonly IPassFileContext<PwdPassFile> _pfContext = 
        Locator.Current.Resolve<IPassFileContextProvider>().For<PwdPassFile>();
    private readonly IPassFileDecryptionHelper _pfDecryptionHelper = 
        Locator.Current.Resolve<IPassFileDecryptionHelper>();

    public PassFileWinViewModel(PwdPassFile passFile)
    {
        PassFile = passFile;
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
            : string.Format(pf.IsLocalCreated()
                ? Resources.PASSFILE__TITLE_NEW
                : pf.IsLocalDeleted()
                    ? Resources.PASSFILE__TITLE_DELETED
                    : Resources.PASSFILE__TITLE, pf.GetIdentityString()));

        CreatedOn = passFileChanged.Select(pf => pf?.CreatedOn.ToLocalTime().ToShortDateTimeString());

        ChangedOn = passFileChanged.Select(pf => string.Join(" / ",
            new[] { pf?.InfoChangedOn, pf?.VersionChangedOn }
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
            IsVisibleObservable = passFileChanged.Select(pf => pf?.IsLocalDeleted() is false)
        };

        MergeBtn = new BtnState
        {
            CommandObservable = Observable.Return(ReactiveCommand.CreateFromTask(MergeAsync)),
            IsVisibleObservable = passFileChanged.Select(pf => pf?.IsLocalDeleted() is false
                                                               && pf.Mark.HasFlag(PassFileMark.NeedsMerge))
        };

        ExportBtn = new BtnState
        {
            CommandObservable = Observable.Return(ReactiveCommand.CreateFromTask(ExportAsync)),
            IsVisibleObservable = passFileChanged.Select(pf => pf?.IsLocalDeleted() is false)
        };

        RestoreBtn = new BtnState
        {
            CommandObservable = Observable.Return(ReactiveCommand.CreateFromTask(RestoreAsync)),
            IsVisibleObservable = passFileChanged.Select(pf => pf is not null)
        };

        DeleteBtn = new BtnState
        {
            CommandObservable = Observable.Return(ReactiveCommand.Create(Delete)),
            IsVisibleObservable = passFileChanged.Select(pf => pf?.IsLocalDeleted() is false)
        };

        ReadOnly = passFileChanged.Select(pf => pf?.IsLocalDeleted() is not false);
    }

    private static string _MakeState(PassFile passFile)
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

    public void Close() => ViewElements.Window!.Close();

    public async Task ChangePasswordAsync()
    {
        if (PassFile?.IsLocalDeleted() is not false) return;

        var result = await _pfDecryptionHelper.ProvideDecryptedContentAsync(PassFile, _pfContext,
            (Resources.PASSFILE__ASK_PASSPHRASE_OLD, null));
        if (result.Bad) return;

        var provideResult = await _pfContext.ProvideEncryptedContentAsync(PassFile);
        if (provideResult.Bad) return;

        var passPhraseNew = await _dialogService.AskPasswordAsync(Resources.PASSFILE__ASK_PASSPHRASE_NEW);
        if (passPhraseNew.Bad || passPhraseNew.Data == string.Empty) return;

        PassFile.Content = new PassFileContent<List<PwdSection>>(PassFile.Content.Encrypted!, passPhraseNew.Data!);

        var updateResult = _pfContext.UpdateContent(PassFile);
        if (updateResult.Ok)
        {
            _dialogService.ShowInfo(Resources.PASSFILE__INFO_PASSPHRASE_CHANGED);
        }
    }

    private void Save()
    {
        if (PassFile is null)
        {
            Close();
            return;
        }

        if (PassFile.IsLocalDeleted())
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(Name))
        {
            _dialogService.ShowFailure(Resources.PASSFILE__VALIDATION__INCORRECT_NAME);
            return;
        }

        PassFile.Name = Name.Trim();
        PassFile.Color = PassFileColor.List[SelectedColorIndex].Hex;

        _pfContext.UpdateInfo(PassFile);
    }

    private void Delete()
    {
        if (PassFile?.IsLocalDeleted() is not false) return;

        var result = _pfContext.Delete(PassFile);

        if (result.Ok && _pfContext.CurrentList.Contains(PassFile))
        {
            Close();
        }
    }

    private Task ExportAsync()
    {
        if (PassFile?.IsLocalDeleted() is not false)
        {
            return Task.CompletedTask;
        }

        var exportService = Locator.Current.Resolve<IPassFileExportUiService>();

        return exportService.SelectAndExportAsync(PassFile, ViewElements.Window!);
    }

    private async Task RestoreAsync()
    {
        if (PassFile is null) return;

        var restoreService = Locator.Current.Resolve<IPassFileRestoreUiService>();

        await restoreService.SelectAndRestoreAsync(PassFile, _pfContext, ViewElements.Window!);
    }

    private async Task MergeAsync()
    {
        if (PassFile?.IsLocalDeleted() is not false) return;

        var mergeService = Locator.Current.Resolve<IPassFileMergeUiService>();

        await mergeService.LoadRemoteAndMergeAsync(PassFile, _pfContext, ViewElements.Window!);
    }

#pragma warning disable 8618
    public PassFileWinViewModel()
    {
    }
#pragma warning restore 8618
}