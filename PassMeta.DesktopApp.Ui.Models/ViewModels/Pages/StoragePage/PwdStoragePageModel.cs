using System;
using System.Linq;
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
using PassMeta.DesktopApp.Ui.Models.Providers;
using PassMeta.DesktopApp.Ui.Models.ViewModels.Base;
using PassMeta.DesktopApp.Ui.Models.ViewModels.Pages.StoragePage.Components;
using PassMeta.DesktopApp.Ui.Models.ViewModels.Pages.StoragePage.Extra;
using ReactiveUI;
using Splat;

namespace PassMeta.DesktopApp.Ui.Models.ViewModels.Pages.StoragePage;

/// <summary>
/// Passfile storage page ViewModel.
/// </summary>
public class PwdStoragePageModel : PageViewModel
{
    private static bool _loaded;
    private static readonly PassFileItemPath LastItemPath = new();

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

    public PassFileListModel<PwdPassFile> PassFileList { get; }

    public PassFileData SelectedData { get; }

    public PassFileBarExpander PassFileBarExpander { get; } = new();

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
    public PwdStoragePageModel(IScreen hostScreen, HostWindowProvider windowProvider) : base(hostScreen)
    {
        PassFileList = new PassFileListModel<PwdPassFile>(windowProvider);
        
        PassFileList
            .WhenAnyValue(x => x.SelectedIndex)
            .Subscribe(async _ =>
            {
                var passFile = PassFileList.GetSelectedPassFile();
                if (passFile is null)
                {
                    return;
                }

                if (passFile.Content.Decrypted is null)
                {
                    var result = await _pfDecryptionHelper.ProvideDecryptedContentAsync(passFile, _pfContext);
                    if (result.Bad)
                    {
                        PassFileList.RollbackSelectedPassFile();
                        return;
                    }
                }
                
                // TODO: show sections
            });

        // LayoutState = this.WhenAnyValue(vm => vm.PassFilesSelectedIndex,
        //         vm => vm.SelectedData.SelectedSectionIndex)
        //     .Select(x => x.Item1 < 0
        //         ? InitLayoutState
        //         : x.Item2 < 0
        //             ? AfterPassFileSelectionLayoutState
        //             : AfterSectionSelectionLayoutState);

        var lastItemPath = LastItemPath.Copy();

        SelectedData = new PassFileData(ViewElements, LastItemPath, PassFileBarExpander);

        SelectedData.WhenAnyValue(vm => vm.SelectedSectionIndex)
            .Subscribe(index => PassFileBarExpander.TryExecuteAutoExpanding(index == -1));

        this.WhenAnyValue(vm => vm.PassFileList.List)
            .Subscribe(_ => SelectedData.PassFile = null);

        // this.WhenAnyValue(vm => vm.PassFilesSelectedIndex)
        //     .InvokeCommand(ReactiveCommand.CreateFromTask<int>(DecryptIfRequiredAndSetSectionsAsync));

        SelectedData.WhenAnyValue(vm => vm.SelectedSectionIndex)
            .Subscribe(index =>
            {
                if (index < 0)
                    PassFileBarExpander.AutoExpanding = true;
            });

        // PassFileBarExpander.IsOpenedObservable
        //     .Subscribe(isOpened =>
        //     {
        //         PassFileBarExpander.AutoExpanding = !(isOpened && _passFilesSelectedIndex >= 0);
        //     });

        this.WhenNavigatedToObservable()
            .InvokeCommand(ReactiveCommand.Create(() => LoadPassFilesAsync(lastItemPath)));
    }

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


    private async Task LoadPassFilesAsync(PassFileItemPath lastItemPath)
    {
        // using var preloader = Locator.Current.Resolve<AppLoading>().General.Begin();
        //
        // if (!_loaded)
        // {
        //     await _pfSyncService.SynchronizeAsync(_pfContext);
        //     _loaded = true;
        // }
        //
        // UpdatePassFileList();
        //
        // if (lastItemPath.PassFileId is not null)
        // {
        //     var index = _passFileList.FindIndex(btn => btn.PassFile!.Id == lastItemPath.PassFileId.Value);
        //     if (index >= 0)
        //     {
        //         if ((_passFileList[index].PassFile as PwdPassFile).Content.PassPhrase is not null)
        //         {
        //             PassFilesSelectedIndex = index;
        //             if (lastItemPath.PassFileSectionId is not null)
        //             {
        //                 SelectedData.SelectedSectionIndex =
        //                     SelectedData.SectionsList!.FindIndex(
        //                         btn => btn.Section.Id == lastItemPath.PassFileSectionId);
        //             }
        //         }
        //     }
        // }
        //
        // PassFileBarExpander.IsOpened = true;
    }

    private async Task DecryptIfRequiredAndSetSectionsAsync(int _)
    {
        // var passFile = SelectedPassFile;
        // if (passFile is null || passFile.Content.Encrypted is not null)
        // {
        //     SelectedData.PassFile = passFile;
        //     return;
        // }
        //
        // if (!passFile.IsLocalDeleted())
        // {
        //     using var preloader = Locator.Current.Resolve<AppLoading>().General.Begin();
        //
        //     var result = await _pfDecryptionHelper.ProvideDecryptedContentAsync(passFile, _pfContext);
        //     if (result.Ok)
        //     {
        //         SelectedData.PassFile = passFile;
        //         return;
        //     }
        // }
        //
        // PassFilesSelectedIndex = _passFilesPrevSelectedIndex;
    }

    private async Task SaveAsync()
    {
        using (Locator.Current.Resolve<AppLoading>().General.Begin())
        {
            await _pfSyncService.SynchronizeAsync(_pfContext);
        }

        await LoadPassFilesAsync(LastItemPath.Copy());
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