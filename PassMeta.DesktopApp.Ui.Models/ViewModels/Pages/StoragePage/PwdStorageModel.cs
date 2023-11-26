using System;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Threading;
using PassMeta.DesktopApp.Common;
using PassMeta.DesktopApp.Common.Abstractions;
using PassMeta.DesktopApp.Common.Abstractions.App;
using PassMeta.DesktopApp.Common.Abstractions.PassFileContext;
using PassMeta.DesktopApp.Common.Abstractions.Services;
using PassMeta.DesktopApp.Common.Abstractions.Services.PassFileServices;
using PassMeta.DesktopApp.Common.Abstractions.Utils.Helpers;
using PassMeta.DesktopApp.Common.Enums;
using PassMeta.DesktopApp.Common.Extensions;
using PassMeta.DesktopApp.Common.Models;
using PassMeta.DesktopApp.Common.Models.Entities.PassFile;
using PassMeta.DesktopApp.Common.Models.Internal;
using PassMeta.DesktopApp.Ui.Models.Abstractions.Providers;
using PassMeta.DesktopApp.Ui.Models.ViewModels.Base;
using PassMeta.DesktopApp.Ui.Models.ViewModels.Pages.Account;
using PassMeta.DesktopApp.Ui.Models.ViewModels.Pages.StoragePage.Components;
using PassMeta.DesktopApp.Ui.Models.ViewModels.Pages.StoragePage.Extra;
using ReactiveUI;
using Splat;

namespace PassMeta.DesktopApp.Ui.Models.ViewModels.Pages.StoragePage;

/// <summary>
/// Passfile storage page ViewModel.
/// </summary>
public class PwdStorageModel : PageViewModel
{
    private readonly IPassFileSyncService _pfSyncService = Locator.Current.Resolve<IPassFileSyncService>();
    private readonly IDialogService _dialogService = Locator.Current.Resolve<IDialogService>();
    private readonly IUserContext _userContext = Locator.Current.Resolve<IUserContextProvider>().Current;
    private readonly AppLoading _appLoading = Locator.Current.Resolve<AppLoading>();

    private readonly IPassFileContext<PwdPassFile> _pfContext =
        Locator.Current.Resolve<IPassFileContextProvider>().For<PwdPassFile>();

    private readonly IPassFileDecryptionHelper _pfDecryptionHelper =
        Locator.Current.Resolve<IPassFileDecryptionHelper>();

    public PwdStorageModel(IScreen hostScreen, IHostWindowProvider windowProvider) : base(hostScreen)
    {
        SectionRead = new PwdSectionReadModel();
        SectionEdit = new PwdSectionEditModel();
        
        SectionList = new PwdSectionListModel();
        SectionList
            .WhenAnyValue(x => x.SelectedIndex)
            .Subscribe(_ => LoadSelectedSection());
        
        PassFileList = new PassFileListModel<PwdPassFile>(windowProvider);
        PassFileList
            .WhenAnyValue(x => x.SelectedIndex)
            .Subscribe(_ => LoadSelectedPassFile());

        LayoutState = this.WhenAnyValue(
                vm => vm.PassFileList.SelectedIndex,
                vm => vm.SectionList.SelectedIndex)
            .Select(x => x.Item1 < 0 
                ? PwdStorageLayoutState.Init
                : x.Item2 < 0
                    ? PwdStorageLayoutState.AfterPassFileSelection
                    : PwdStorageLayoutState.AfterSectionSelection);

        PassFileBarExpander.IsOpenedObservable.Subscribe(isOpened => PassFileList.IsExpanded = isOpened);

        Task.Run(SynchronizePassFilesAsync);
    }
    
    /// <inheritdoc />
    public override ContentControl[] RightBarButtons => new ContentControl[]
    {
        new Button
        {
            Content = "\uE74E",
            Command = ReactiveCommand.CreateFromTask(SynchronizePassFilesAsync),
            [!Visual.IsVisibleProperty] = SectionEdit.WhenAnyValue(vm => vm.IsVisible)
                .Select(editVisible => !editVisible)
                .ToBinding(),
            [!InputElement.IsEnabledProperty] = _pfContext.AnyChangedSource
                .ObserveOn(RxApp.MainThreadScheduler)
                .ToBinding(),
            [ToolTip.TipProperty] = Resources.STORAGE__RIGHT_BAR_TOOLTIP__SAVE,
            [ToolTip.PlacementProperty] = PlacementMode.Left
        },
        new Button
        {
            Content = "\uE70F",
            Command = ReactiveCommand.Create(BeginSectionChanges),
            [!Visual.IsVisibleProperty] = SectionRead.WhenAnyValue(vm => vm.IsVisible)
                .ToBinding(),
            [ToolTip.TipProperty] = Resources.STORAGE__RIGHT_BAR_TOOLTIP__EDIT_ITEMS,
            [ToolTip.PlacementProperty] = PlacementMode.Left
        },
        new Button
        {
            Content = "\uE711",
            Command = ReactiveCommand.Create(DiscardSectionChanges),
            [!Visual.IsVisibleProperty] = SectionEdit.WhenAnyValue(vm => vm.IsVisible)
                .ToBinding(),
            [ToolTip.TipProperty] = Resources.STORAGE__RIGHT_BAR_TOOLTIP__DISCARD_ITEMS,
            [ToolTip.PlacementProperty] = PlacementMode.Left
        },
        new Button
        {
            Content = "\uE8FB",
            Command = ReactiveCommand.Create(ApplySectionChanges),
            [!Visual.IsVisibleProperty] = SectionEdit.WhenAnyValue(vm => vm.IsVisible)
                .ToBinding(),
            [ToolTip.TipProperty] = Resources.STORAGE__RIGHT_BAR_TOOLTIP__APPLY_ITEMS,
            [ToolTip.PlacementProperty] = PlacementMode.Left
        },
        new Button
        {
            Content = "\uE74D",
            Command = ReactiveCommand.Create(DeleteSectionAsync),
            [!Visual.IsVisibleProperty] = SectionRead.WhenAnyValue(vm => vm.IsVisible)
                .ToBinding(),
            [ToolTip.TipProperty] = Resources.STORAGE__RIGHT_BAR_TOOLTIP__DELETE_SECTION,
            [ToolTip.PlacementProperty] = PlacementMode.Left
        },
    };

    public PassFileListModel<PwdPassFile> PassFileList { get; }

    public PwdSectionListModel SectionList { get; }

    public PwdSectionReadModel SectionRead { get; }

    public PwdSectionEditModel SectionEdit { get; }

    public PassFileBarExpander PassFileBarExpander { get; } = new();

    public IObservable<PwdStorageLayoutState> LayoutState { get; }

    /// <inheritdoc />
    protected override async ValueTask<IResult> CanLeaveAsync()
    {
        if (!SectionEdit.IsVisible)
        {
            return Result.Success();
        }

        var result = await _dialogService.ConfirmAsync(Resources.STORAGE__CONFIRM_SECTION_RESET);
        if (result.Ok)
        {
            SectionEdit.Hide();
            SectionRead.Show(SectionList.GetSelectedSection()!);
        }

        return result;
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

        await SynchronizePassFilesAsync();
    }

    private async Task SynchronizePassFilesAsync()
    {
        using var preloader = _appLoading.General.Begin();

        var rememberPath = new PwdStoragePath(
            PassFileList.GetSelectedPassFile()?.Id,
            SectionList.GetSelectedSection()?.Id);

        await _pfSyncService.SynchronizeAsync(_pfContext);

        await Dispatcher.UIThread.InvokeAsync(() => SelectPath(rememberPath));
    }

    private async void LoadSelectedPassFile()
    {
        using var preloader = _appLoading.General.Begin();

        var passFile = PassFileList.GetSelectedPassFile();
        if (passFile is null)
        {
            SectionList.Hide();
            return;
        }

        if (passFile.IsLocalDeleted())
        {
            PassFileList.RollbackSelectedPassFile();
            return;
        }

        var result = await _pfDecryptionHelper.ProvideDecryptedContentAsync(passFile, _pfContext);
        if (result.Bad)
        {
            PassFileList.RollbackSelectedPassFile();
            return;
        }

        SectionList.SelectedIndex = -1;
        SectionList.Show(passFile.Content.Decrypted!);
    }

    private void LoadSelectedSection()
    {
        var section = SectionList.GetSelectedSection();
        if (section is null)
        {
            SectionRead.Hide();
            SectionEdit.Hide();
        }
        else if (section.Mark.HasFlag(PwdSectionMark.Created))
        {
            SectionRead.Hide();
            SectionEdit.Show(section);
        }
        else
        {
            SectionRead.Show(section);
            SectionEdit.Hide();
        }

        PassFileBarExpander.TryExecuteAutoExpanding(section is null);

        if (section is null)
        {
            PassFileBarExpander.AutoExpanding = true;
        }
    }

    private void SelectPath(PwdStoragePath path)
    {
        PassFileList.SelectedIndex = -1;
        PassFileList.RefreshList(_pfContext.CurrentList);

        if (path.PassFileId is not null)
        {
            var index = PassFileList.List
                .FindIndex(btn => btn.PassFile.Id == path.PassFileId);

            if (index >= 0)
            {
                var passFile = (PwdPassFile) PassFileList.List[index].PassFile;

                if (passFile.Content.PassPhrase is not null)
                {
                    PassFileList.SelectedIndex = index;

                    if (path.SectionId is not null)
                    {
                        SectionList.SelectedIndex = SectionList.List
                            .FindIndex(btn => btn.Section.Id == path.SectionId);
                    }
                }
            }
        }

        PassFileBarExpander.IsOpened = true;
    }

    #region Mutations

    private async Task DeleteSectionAsync()
    {
        var passFile = PassFileList.GetSelectedPassFile()!;
        var section = SectionList.GetSelectedSection()!;

        var confirm =
            await _dialogService.ConfirmAsync(string.Format(Resources.STORAGE__CONFIRM_DELETE_SECTION, section.Name));
        if (confirm.Bad) return;

        using var preloader = Locator.Current.Resolve<AppLoading>().General.Begin();

        passFile.Content.Decrypted!.RemoveAll(s => s.Id == section.Id);

        var result = _pfContext.UpdateContent(passFile);
        if (result.Ok)
        {
            SectionList.Remove(section);
        }
    }

    private void BeginSectionChanges()
    {
        SectionRead.Hide();
        SectionEdit.Show(SectionList.GetSelectedSection()!);
    }

    private void ApplySectionChanges()
    {
        using var preloader = Locator.Current.Resolve<AppLoading>().General.Begin();

        var passFile = PassFileList.GetSelectedPassFile()!;
        var section = SectionList.GetSelectedSection()!;

        var sectionActual = SectionEdit.ToSection();

        if (!section.Mark.HasFlag(PwdSectionMark.Created) && !sectionActual.DiffersFrom(section))
        {
            SectionEdit.Hide();
            SectionRead.Show(sectionActual);
            return;
        }

        passFile.Content.Decrypted!.RemoveAll(x => x.Id == sectionActual.Id);
        passFile.Content.Decrypted.Add(sectionActual);

        var result = _pfContext.UpdateContent(passFile);
        if (result.Bad)
        {
            return;
        }

        using (PassFileBarExpander.DisableAutoExpandingScoped())
        {
            SectionList.RefreshOnly(sectionActual);
        }

        SectionEdit.Hide();
        SectionRead.Show(sectionActual);
    }

    private void DiscardSectionChanges()
    {
        using (PassFileBarExpander.DisableAutoExpandingScoped())
        {
            var section = SectionList.GetSelectedSection()!;

            SectionEdit.Hide();

            if (section.Mark.HasFlag(PwdSectionMark.Created))
            {
                SectionList.Remove(section);
            }
            else
            {
                SectionRead.Show(section);
            }
        }
    }

    #endregion
    
    private record struct PwdStoragePath(long? PassFileId, Guid? SectionId);
}