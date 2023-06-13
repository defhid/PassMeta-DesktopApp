using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using PassMeta.DesktopApp.Common;
using PassMeta.DesktopApp.Common.Abstractions.PassFileContext;
using PassMeta.DesktopApp.Common.Abstractions.Services;
using PassMeta.DesktopApp.Common.Extensions;
using PassMeta.DesktopApp.Common.Models.App;
using PassMeta.DesktopApp.Common.Models.Entities.PassFile;
using PassMeta.DesktopApp.Common.Models.Entities.PassFile.Extra;
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
    private readonly IDialogService _dialogService = Locator.Current.Resolve<IDialogService>();
    private readonly HostWindowProvider _windowProvider;
    private readonly IPassFileContext<TPassFile> _pfContext;

    private IReadOnlyList<PassFileCellModel<TPassFile>> _list = Array.Empty<PassFileCellModel<TPassFile>>();
    private int _selectedIndex = -1;
    private int _prevSelectedIndex = -1;
    private bool _isExpanded;
    private readonly IObservable<bool> _fullModeObservable;
    private bool _isReadOnly;

    /// <summary></summary>
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

    /// <summary></summary>
    public BtnState ExpanderBtn { get; }

    /// <summary></summary>
    public bool IsExpanded
    {
        get => _isExpanded;
        set => this.RaiseAndSetIfChanged(ref _isExpanded, value);
    }

    /// <summary></summary>
    public bool IsReadOnly
    {
        get => _isReadOnly;
        set => this.RaiseAndSetIfChanged(ref _isReadOnly, value);
    }

    /// <summary>
    /// List of passfiles to display.
    /// </summary>
    public IReadOnlyList<PassFileCellModel<TPassFile>> List
    {
        get => _list;
        private set => this.RaiseAndSetIfChanged(ref _list, value);
    }

    /// <summary></summary>
    public int SelectedIndex
    {
        get => _selectedIndex;
        set
        {
            _prevSelectedIndex = _selectedIndex;
            this.RaiseAndSetIfChanged(ref _selectedIndex, value);
        }
    }

    /// <summary></summary>
    public TPassFile? GetSelectedPassFile()
        => _selectedIndex < 0 ? null : _list[_selectedIndex].PassFile;

    /// <summary></summary>
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

        SelectedIndex = List.FindIndex(x => x.PassFile == result.Data);

        Debug.Assert(SelectedIndex >= 0);

        if (SelectedIndex >= 0)
        {
            await List[SelectedIndex].ShowCardAsync();
        }
    }

    /// <summary>
    /// Actualize list from current passfile context.
    /// </summary>
    public void RefreshList(IEnumerable<TPassFile> passFiles)
    {
        List = passFiles
            .OrderBy(x => x, PassFileComparer.Instance)
            .Select(ToPassFileCell)
            .ToList();
    }

    private PassFileCellModel<TPassFile> ToPassFileCell(TPassFile passFile)
        => new(passFile, _fullModeObservable, _windowProvider);
}