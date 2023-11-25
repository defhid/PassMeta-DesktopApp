using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Avalonia.Controls;
using Avalonia.Media;
using PassMeta.DesktopApp.Common;
using PassMeta.DesktopApp.Common.Abstractions.PassFileContext;
using PassMeta.DesktopApp.Common.Abstractions.Services;
using PassMeta.DesktopApp.Common.Abstractions.Utils.Helpers;
using PassMeta.DesktopApp.Common.Enums;
using PassMeta.DesktopApp.Common.Extensions;
using PassMeta.DesktopApp.Common.Mapping.Values;
using PassMeta.DesktopApp.Common.Models.Entities.PassFile;
using PassMeta.DesktopApp.Ui.Models.Abstractions.Services;
using PassMeta.DesktopApp.Ui.Models.Constants;
using PassMeta.DesktopApp.Ui.Models.Extensions;
using PassMeta.DesktopApp.Ui.Models.Providers;
using PassMeta.DesktopApp.Ui.Models.ViewModels.Common;
using ReactiveUI;
using Splat;

namespace PassMeta.DesktopApp.Ui.Models.ViewModels.Windows.PassFileWin;

/// <inheritdoc />
public class PassFileWinModel<TPassFile> : ReactiveObject
    where TPassFile : PassFile
{
    private readonly IMapper _mapper;
    private readonly IDialogService _dialogService;
    private readonly IPassFileContext<TPassFile> _pfContext;
    private readonly IPassFileContentHelper<TPassFile> _pfContentHelper;
    private readonly HostWindowProvider _hostWindowProvider;

    private string? _name;
    private int _selectedColorIndex;
    private TPassFile _passFileOrigin;

    public readonly TPassFile PassFile;
    public event Action? Finish;

    public PassFileWinModel(TPassFile passFile, HostWindowProvider hostWindowProvider)
    {
        _mapper = Locator.Current.Resolve<IMapper>();
        _dialogService = Locator.Current.Resolve<IDialogService>();
        _pfContext = Locator.Current.Resolve<IPassFileContextProvider>().For<TPassFile>();
        _pfContentHelper = Locator.Current.Resolve<IPassFileContentHelper<TPassFile>>();
        _hostWindowProvider = hostWindowProvider;

        _passFileOrigin = _mapper.Map<TPassFile, TPassFile>(passFile);
        _name = passFile.Name;
        _selectedColorIndex = PassFileColor.List.IndexOf(passFile.GetPassFileColor());

        PassFile = passFile;
        CloseCommand = ReactiveCommand.Create(() => Finish?.Invoke());

        var passFileChanged = this.WhenAnyValue(vm => vm.PassFile);

        var anyChanged = this.WhenAnyValue(
                vm => vm.Name,
                vm => vm.SelectedColorIndex,
                vm => vm.PassFileOrigin)
            .Select(val =>
                val.Item1 != val.Item3.Name ||
                PassFileColor.List[val.Item2] != val.Item3.GetPassFileColor());

        Title = passFileChanged.Select(pf => string.Format(pf.IsLocalCreated()
            ? Resources.PASSFILE__TITLE_NEW
            : pf.IsLocalDeleted()
                ? Resources.PASSFILE__TITLE_DELETED
                : Resources.PASSFILE__TITLE, pf.GetIdentityString()));

        CreatedOn = passFileChanged.Select(pf => pf.CreatedOn.ToLocalTime().ToShortDateTimeString());

        ChangedOn = passFileChanged.Select(pf => string.Join(" / ",
            new[] { pf.InfoChangedOn, pf.VersionChangedOn }
                .Select(dt => dt.ToLocalTime().ToShortDateTimeString())));

        StateColor = passFileChanged.Select(pf => pf.GetStateColor());

        State = passFileChanged.Select(_MakeState);

        OkBtn = new BtnState
        {
            ContentObservable = anyChanged.Select(changed => changed
                ? Resources.PASSFILE__BTN_SAVE
                : Resources.PASSFILE__BTN_OK),
            CommandObservable = anyChanged.Select(changed => changed ? ReactiveCommand.Create(Save) : CloseCommand)
        };

        ChangePasswordBtn = new BtnState
        {
            CommandObservable = Observable.Return(ReactiveCommand.CreateFromTask(ChangePasswordAsync)),
            IsVisibleObservable = passFileChanged.Select(pf => !pf.IsLocalDeleted())
        };

        MergeBtn = new BtnState
        {
            CommandObservable = Observable.Return(ReactiveCommand.CreateFromTask(MergeAsync)),
            IsVisibleObservable = passFileChanged.Select(pf => !pf.IsLocalDeleted() &&
                                                               pf.Mark.HasFlag(PassFileMark.NeedsMerge))
        };

        ExportBtn = new BtnState
        {
            CommandObservable = Observable.Return(ReactiveCommand.CreateFromTask(ExportAsync)),
            IsVisibleObservable = passFileChanged.Select(pf => !pf.IsLocalDeleted())
        };

        RestoreBtn = new BtnState
        {
            CommandObservable = Observable.Return(ReactiveCommand.CreateFromTask(RestoreAsync))
        };

        DeleteBtn = new BtnState
        {
            CommandObservable = Observable.Return(ReactiveCommand.Create(Delete)),
            IsVisibleObservable = passFileChanged.Select(pf => !pf.IsLocalDeleted())
        };

        ReadOnly = passFileChanged.Select(pf => pf.IsLocalDeleted());
    }

    public IObservable<string> Title { get; }
    public IObservable<bool> ReadOnly { get; }
    public IObservable<string?> CreatedOn { get; }
    public IObservable<string?> ChangedOn { get; }
    public IObservable<ISolidColorBrush> StateColor { get; }
    public IObservable<string> State { get; }

    private TPassFile PassFileOrigin
    {
        get => _passFileOrigin;
        set => this.RaiseAndSetIfChanged(ref _passFileOrigin, value);
    }

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

    public BtnState OkBtn { get; }
    public BtnState ChangePasswordBtn { get; }
    public BtnState MergeBtn { get; }
    public BtnState ExportBtn { get; }
    public BtnState RestoreBtn { get; }
    public BtnState DeleteBtn { get; }

    public ReactiveCommand<Unit, Unit> CloseCommand { get; }

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

    private async Task ChangePasswordAsync()
    {
        if (PassFile.IsLocalDeleted())
        {
            return;
        }

        var result = await _pfContentHelper.ChangePassPhraseAsync(PassFile);
        if (result.Ok)
        {
            _dialogService.ShowInfo(Resources.PASSFILE__INFO_PASSPHRASE_CHANGED);
        }
        else
        {
            _dialogService.ShowWarning(Resources.PASSFILE__INFO_PASSPHRASE_NOT_CHANGED);
        }
    }

    private void Save()
    {
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

        var result = _pfContext.UpdateInfo(PassFile);
        if (result.Ok)
        {
            PassFileOrigin = _mapper.Map<TPassFile, TPassFile>(PassFile);
        }
    }

    private void Delete()
    {
        if (PassFile.IsLocalDeleted())
        {
            return;
        }

        var result = _pfContext.Delete(PassFile);

        if (result.Ok && !_pfContext.CurrentList.Contains(PassFile))
        {
            _ = CloseCommand.Execute();
        }
    }

    private async Task ExportAsync()
    {
        if (PassFile.IsLocalDeleted())
        {
            return;
        }

        var exportService = Locator.Current.Resolve<IPassFileExportUiService<TPassFile>>();

        //await exportService.SelectAndExportAsync(PassFile, TopLevel.GetTopLevel().StorageProvider);
    }

    private async Task RestoreAsync()
    {
        var restoreService = Locator.Current.Resolve<IPassFileRestoreUiService<TPassFile>>();

        await restoreService.SelectAndRestoreAsync(PassFile, _pfContext, _hostWindowProvider);
    }

    private async Task MergeAsync()
    {
        if (PassFile.IsLocalDeleted())
        {
            return;
        }

        var mergeService = Locator.Current.Resolve<IPassFileMergeUiService<TPassFile>>();

        await mergeService.LoadRemoteAndMergeAsync(PassFile, _pfContext, _hostWindowProvider);
    }
}