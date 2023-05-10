using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using PassMeta.DesktopApp.Common;
using PassMeta.DesktopApp.Common.Abstractions;
using PassMeta.DesktopApp.Common.Abstractions.App;
using PassMeta.DesktopApp.Common.Abstractions.PassFileContext;
using PassMeta.DesktopApp.Common.Abstractions.Services;
using PassMeta.DesktopApp.Common.Abstractions.Services.PassFileServices;
using PassMeta.DesktopApp.Common.Abstractions.Utils.Helpers;
using PassMeta.DesktopApp.Common.Extensions;
using PassMeta.DesktopApp.Common.Models;
using PassMeta.DesktopApp.Common.Models.App;
using PassMeta.DesktopApp.Common.Models.Entities.PassFile;
using PassMeta.DesktopApp.Common.Models.Entities.PassFile.Data;
using PassMeta.DesktopApp.Ui.Models.Comparers;
using PassMeta.DesktopApp.Ui.Models.Constants;
using PassMeta.DesktopApp.Ui.Models.ViewModels.Base;
using PassMeta.DesktopApp.Ui.Models.ViewModels.Common;
using PassMeta.DesktopApp.Ui.Models.ViewModels.Pages.StoragePage.Components;
using PassMeta.DesktopApp.Ui.Models.ViewModels.Pages.StoragePage.Extra;
using PassMeta.DesktopApp.Ui.Models.ViewModels.Windows.PassFileWin;
using ReactiveUI;
using Splat;

namespace PassMeta.DesktopApp.Ui.Models.ViewModels.Pages.StoragePage;

/// <summary>
/// Passfile storage page ViewModel.
/// </summary>
public class StoragePageModel : PageViewModel
{
    private static bool _loaded;
    private static readonly PassFileItemPath LastItemPath = new();

    public readonly Interaction<PassFileWinViewModel<PwdPassFile>, Unit> ShowPassFile = new();

    /// <inheritdoc />
    public override ContentControl[] RightBarButtons => new ContentControl[]
    {
        new Button
        {
            Content = "\uE74E",
            Command = ReactiveCommand.CreateFromTask(SaveAsync),
            [!Visual.IsVisibleProperty] = SelectedData.Edit.WhenAnyValue(vm => vm.Mode)
                .Select(editMode => !editMode)
                .ToBinding(),
            [!InputElement.IsEnabledProperty] = Locator.Current.Resolve<IPassFileContextProvider>()
                .For<PwdPassFile>()
                .AnyChangedSource
                .Select(state => state)
                .ToBinding(),
            [ToolTip.TipProperty] = Resources.STORAGE__RIGHT_BAR_TOOLTIP__SAVE,
            [ToolTip.PlacementProperty] = PlacementMode.Left
        }
    }.Concat(SelectedData.RightBarButtons).ToArray();

    #region Passfile list

    private ObservableCollection<PassFileCellModel<PwdPassFile>> _passFileList = new();

    public ObservableCollection<PassFileCellModel<PwdPassFile>> PassFileList
    {
        get => _passFileList;
        private set => this.RaiseAndSetIfChanged(ref _passFileList, value);
    }

    private int _passFilesSelectedIndex = -1;
    private int _passFilesPrevSelectedIndex = -1;

    public int PassFilesSelectedIndex
    {
        get => _passFilesSelectedIndex;
        set
        {
            _passFilesPrevSelectedIndex = _passFilesSelectedIndex;
            this.RaiseAndSetIfChanged(ref _passFilesSelectedIndex, value);
        }
    }

    public PassFileCellModel<PwdPassFile>? SelectedPassFileBtn =>
        _passFilesSelectedIndex == -1 ? null : _passFileList[_passFilesSelectedIndex];

    public PwdPassFile? SelectedPassFile =>
        _passFilesSelectedIndex == -1 ? null : (PwdPassFile) _passFileList[_passFilesSelectedIndex].PassFile;

    #endregion

    public PassFileData SelectedData { get; }

    public PassFileBarExpander PassFileBarExpander { get; }

    public BtnState PassFilesBarBtn { get; }

    public IObservable<LayoutState> LayoutState { get; }

    public readonly ViewElements ViewElements = new();

    private readonly IPassFileSyncService _pfSyncService = Locator.Current.Resolve<IPassFileSyncService>();
    private readonly IDialogService _dialogService = Locator.Current.Resolve<IDialogService>();
    private readonly IUserContext _userContext = Locator.Current.Resolve<IUserContextProvider>().Current;

    private readonly IPassFileContext<PwdPassFile> _pfContext =
        Locator.Current.Resolve<IPassFileContextProvider>().For<PwdPassFile>();

    private readonly IPassFileDecryptionHelper _pfDecryptionHelper =
        Locator.Current.Resolve<IPassFileDecryptionHelper>();

    /// <summary></summary>
    public StoragePageModel(IScreen hostScreen) : base(hostScreen)
    {
        PassFileBarExpander = new PassFileBarExpander();

        PassFilesBarBtn = new BtnState
        {
            ContentObservable = PassFileBarExpander.IsOpenedObservable.Select(isOpened => isOpened
                ? Resources.STORAGE__TITLE
                : "\uE92D"),
            FontFamilyObservable = PassFileBarExpander.IsOpenedObservable.Select(isOpened => isOpened
                ? FontFamilies.Default
                : FontFamilies.SegoeMdl2),
            FontSizeObservable = PassFileBarExpander.IsOpenedObservable.Select(isOpened => isOpened
                ? 18d
                : 22d),
            RotationAngleObservable = PassFileBarExpander.IsOpenedObservable.Select(isOpened => isOpened
                ? 0
                : 45),
        };

        LayoutState = this.WhenAnyValue(vm => vm.PassFilesSelectedIndex,
                vm => vm.SelectedData.SelectedSectionIndex)
            .Select(x => x.Item1 < 0
                ? InitLayoutState
                : x.Item2 < 0
                    ? AfterPassFileSelectionLayoutState
                    : AfterSectionSelectionLayoutState);

        var lastItemPath = LastItemPath.Copy();

        SelectedData = new PassFileData(ViewElements, LastItemPath, PassFileBarExpander);

        SelectedData.WhenAnyValue(vm => vm.SelectedSectionIndex)
            .Subscribe(index => PassFileBarExpander.TryExecuteAutoExpanding(index == -1));

        this.WhenAnyValue(vm => vm.PassFileList)
            .Subscribe(_ => SelectedData.PassFile = null);

        this.WhenAnyValue(vm => vm.PassFilesSelectedIndex)
            .InvokeCommand(ReactiveCommand.CreateFromTask<int>(DecryptIfRequiredAndSetSectionsAsync));

        SelectedData.WhenAnyValue(vm => vm.SelectedSectionIndex)
            .Subscribe(index =>
            {
                if (index < 0)
                    PassFileBarExpander.AutoExpanding = true;
            });

        PassFileBarExpander.IsOpenedObservable
            .Subscribe(isOpened =>
            {
                PassFileBarExpander.AutoExpanding = !(isOpened && _passFilesSelectedIndex >= 0);
            });

        this.WhenNavigatedToObservable()
            .InvokeCommand(ReactiveCommand.Create(() =>
                _pfContext.AnyChangedSource.Subscribe(_ => LoadPassFilesAsync(lastItemPath))));
    }

    #region preview

    /// <summary></summary>
    [Obsolete("PREVIEW constructor")]
    public StoragePageModel() : this(null!)
    {
    }

    #endregion

    /// <inheritdoc />
    protected override async ValueTask<IResult> CanLeaveAsync()
    {
        return SelectedData.Edit.Mode
            ? await _dialogService.ConfirmAsync(Resources.STORAGE__CONFIRM_SECTION_RESET)
            : Result.Failure();
    }

    /// <inheritdoc />
    public override async ValueTask RefreshAsync()
    {
        if (_userContext.UserId is null)
        {
            await new AuthPageModel(HostScreen).TryNavigateAsync();
            return;
        }

        if (_pfContext.AnyChanged)
        {
            var confirm = await _dialogService.ConfirmAsync(Resources.STORAGE__CONFIRM_ROLLBACK);
            if (confirm.Bad) return;

            _pfContext.Rollback();
        }

        _loaded = false;
        await LoadPassFilesAsync(LastItemPath.Copy());
    }

    private PassFileCellModel<PwdPassFile> _MakePassFileBtn(PwdPassFile passFile)
        => new(passFile, ShowPassFile, PassFileBarExpander.ShortModeObservable);

    private async Task LoadPassFilesAsync(PassFileItemPath lastItemPath)
    {
        using var preloader = Locator.Current.Resolve<AppLoading>().General.Begin();

        if (!_loaded)
        {
            await _pfSyncService.SynchronizeAsync(_pfContext);
            _loaded = true;
        }

        UpdatePassFileList();

        if (lastItemPath.PassFileId is not null)
        {
            var index = _passFileList.FindIndex(btn => btn.PassFile!.Id == lastItemPath.PassFileId.Value);
            if (index >= 0)
            {
                if ((_passFileList[index].PassFile as PwdPassFile).Content.PassPhrase is not null)
                {
                    PassFilesSelectedIndex = index;
                    if (lastItemPath.PassFileSectionId is not null)
                    {
                        SelectedData.SelectedSectionIndex =
                            SelectedData.SectionsList!.FindIndex(
                                btn => btn.Section.Id == lastItemPath.PassFileSectionId);
                    }
                }
            }
        }

        PassFileBarExpander.IsOpened = true;
    }

    private void UpdatePassFileList()
    {
        PassFileList = new ObservableCollection<PassFileCellModel<PwdPassFile>>(_pfContext.CurrentList
            .OrderBy(x => x, PassFileComparer.Instance)
            .Select(_MakePassFileBtn));
    }

    private async Task DecryptIfRequiredAndSetSectionsAsync(int _)
    {
        var passFile = SelectedPassFile;
        if (passFile is null || passFile.Content.Encrypted is not null)
        {
            SelectedData.PassFile = passFile;
            return;
        }

        if (!passFile.IsLocalDeleted())
        {
            using var preloader = Locator.Current.Resolve<AppLoading>().General.Begin();

            var result = await _pfDecryptionHelper.ProvideDecryptedContentAsync(passFile, _pfContext);
            if (result.Ok)
            {
                SelectedData.PassFile = passFile;
                return;
            }
        }

        PassFilesSelectedIndex = _passFilesPrevSelectedIndex;
    }

    private async Task SaveAsync()
    {
        using (Locator.Current.Resolve<AppLoading>().General.Begin())
        {
            await _pfSyncService.SynchronizeAsync(_pfContext);
        }

        await LoadPassFilesAsync(LastItemPath.Copy());
    }

    public async Task PassFileAddAsync()
    {
        using var preloader = Locator.Current.Resolve<AppLoading>().General.Begin();

        var askPassPhrase = await _dialogService.AskPasswordAsync(Resources.STORAGE__ASK_PASSPHRASE_FOR_NEW_PASSFILE);
        if (askPassPhrase.Bad || askPassPhrase.Data == string.Empty) return;

        var passFile = await _pfContext.CreateAsync();
        passFile.Content = new PassFileContent<List<PwdSection>>(new List<PwdSection>(), askPassPhrase.Data!);

        var passFileBtn = _MakePassFileBtn(passFile);

        PassFileList.Insert(0, passFileBtn);
        PassFilesSelectedIndex = 0;

        passFileBtn.OpenCard();
    }

    public void PassFileOpen()
    {
        SelectedPassFileBtn!.OpenCard();
    }

    #region Layout states

    private static readonly LayoutState InitLayoutState = new()
    {
        PassFilesPaneWidth = 250d,
        SectionsListMargin = new Thickness(0)
    };

    private static readonly LayoutState AfterPassFileSelectionLayoutState = new()
    {
        PassFilesPaneWidth = 250d,
        SectionsListMargin = new Thickness(0, 0, -100, 0)
    };

    private static readonly LayoutState AfterSectionSelectionLayoutState = new()
    {
        PassFilesPaneWidth = 200d,
        SectionsListMargin = new Thickness(0)
    };

    #endregion
}