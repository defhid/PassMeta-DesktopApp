using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using PassMeta.DesktopApp.Common;
using PassMeta.DesktopApp.Common.Abstractions.PassFileContext;
using PassMeta.DesktopApp.Common.Abstractions.Services;
using PassMeta.DesktopApp.Common.Extensions;
using PassMeta.DesktopApp.Common.Models.Entities.PassFile;
using PassMeta.DesktopApp.Common.Models.Entities.PassFile.Extra;
using PassMeta.DesktopApp.Common.Models.Internal;
using PassMeta.DesktopApp.Ui.Models.Abstractions.Services;
using PassMeta.DesktopApp.Ui.Models.Comparers;
using PassMeta.DesktopApp.Ui.Models.Constants;
using PassMeta.DesktopApp.Ui.Models.Providers;
using PassMeta.DesktopApp.Ui.Models.ViewModels.Common;
using ReactiveUI;
using Splat;

namespace PassMeta.DesktopApp.Ui.Models.ViewModels.Pages.StoragePage.Components;

/// <summary>
/// <see cref="PassFile"/> list ViewModel.
/// </summary>
public class PassFileListModel<TPassFile> : ReactiveObject
    where TPassFile : PassFile
{
    private readonly IPassFileOpenUiService<TPassFile> _pfOpenService =
        Locator.Current.Resolve<IPassFileOpenUiService<TPassFile>>();

    private readonly IDialogService _dialogService = Locator.Current.Resolve<IDialogService>();
    private readonly HostWindowProvider _windowProvider;
    private readonly IPassFileContext<TPassFile> _pfContext;

    private IReadOnlyList<PassFileCellModel> _list = Array.Empty<PassFileCellModel>();
    private int _selectedIndex = -1;
    private int _prevSelectedIndex = -1;
    private bool _isExpanded;
    private readonly IObservable<bool> _fullModeObservable;
    private bool _isReadOnly;

    public PassFileListModel(HostWindowProvider windowProvider)
    {
        _windowProvider = windowProvider;
        _pfContext = Locator.Current.Resolve<IPassFileContextProvider>().For<TPassFile>();
        _fullModeObservable = this.WhenAnyValue(x => x.IsExpanded);

        ExpanderBtn = new BtnState
        {
            ContentObservable = _fullModeObservable.Select(isFull => isFull
                ? Resources.STORAGE__TITLE
                : "\uE92D"),
            FontFamilyObservable = _fullModeObservable.Select(isFull => isFull
                ? FontFamilies.Default
                : FontFamilies.SegoeMdl2),
            FontSizeObservable = _fullModeObservable.Select(isFull => isFull
                ? 18d
                : 22d),
            RotationAngleObservable = _fullModeObservable.Select(isFull => isFull
                ? 0
                : 45),
        };
    }

    public BtnState ExpanderBtn { get; }

    public bool IsExpanded
    {
        get => _isExpanded;
        set => this.RaiseAndSetIfChanged(ref _isExpanded, value);
    }

    public bool IsReadOnly
    {
        get => _isReadOnly;
        set => this.RaiseAndSetIfChanged(ref _isReadOnly, value);
    }

    public IReadOnlyList<PassFileCellModel> List
    {
        get => _list;
        private set => this.RaiseAndSetIfChanged(ref _list, value);
    }

    public int SelectedIndex
    {
        get => _selectedIndex;
        set
        {
            _prevSelectedIndex = _selectedIndex;
            this.RaiseAndSetIfChanged(ref _selectedIndex, value);
        }
    }

    public TPassFile? GetSelectedPassFile()
        => _selectedIndex < 0 ? null : (TPassFile)_list[_selectedIndex].PassFile;

    public void RollbackSelectedPassFile()
        => SelectedIndex = _prevSelectedIndex;

    /// <summary>
    /// Add a new passfile.
    /// </summary>
    public async Task AddAsync()
    {
        using var preloader = Locator.Current.Resolve<AppLoading>().General.Begin();

        var askPassPhrase = await _dialogService
            .AskPasswordAsync(Resources.STORAGE__ASK_PASSPHRASE_FOR_NEW_PASSFILE);

        if (askPassPhrase.Bad)
        {
            return;
        }

        var result = await _pfContext.CreateAsync(new PassFileCreationArgs(askPassPhrase.Data!));
        if (result.Bad)
        {
            return;
        }

        var cell = ToCell(result.Data!);

        List = new[] { cell }.Concat(List).ToList();
        SelectedIndex = 0;

        await ShowCardAsync(cell.PassFile);
    }

    /// <summary>
    /// Actualize list from current passfile context.
    /// </summary>
    public void RefreshList(IEnumerable<TPassFile> passFiles)
        => List = passFiles
            .OrderBy(x => x, PassFileComparer.Instance)
            .Select(ToCell)
            .ToList();

    private async Task ShowCardAsync(PassFile passFile)
    {
        await _pfOpenService.ShowInfoAsync((TPassFile)passFile, _windowProvider);

        if (!_pfContext.CurrentList.Contains(passFile))
        {
            List = List.Where(x => x.PassFile != passFile).ToList();
        }
    }

    private PassFileCellModel ToCell(TPassFile passFile)
        => new(passFile, _fullModeObservable, ReactiveCommand.CreateFromTask(() => ShowCardAsync(passFile)));
}