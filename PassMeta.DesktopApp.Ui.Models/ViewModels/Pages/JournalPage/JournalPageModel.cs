using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using PassMeta.DesktopApp.Common;
using PassMeta.DesktopApp.Common.Abstractions.App;
using PassMeta.DesktopApp.Common.Abstractions.Services;
using PassMeta.DesktopApp.Common.Extensions;
using PassMeta.DesktopApp.Common.Models.App;
using PassMeta.DesktopApp.Common.Models.Dto.Response;
using PassMeta.DesktopApp.Ui.Models.ViewModels.Base;
using PassMeta.DesktopApp.Ui.Models.ViewModels.Pages.Account;
using PassMeta.DesktopApp.Ui.Models.ViewModels.Pages.JournalPage.Extra;
using ReactiveUI;
using Splat;

namespace PassMeta.DesktopApp.Ui.Models.ViewModels.Pages.JournalPage;

/// <summary>
/// Journal page ViewModel.
/// </summary>
public class JournalPageModel : PageViewModel
{
    private readonly IHistoryService _historyService = Locator.Current.Resolve<IHistoryService>();
    private readonly IUserContextProvider _userContextProvider = Locator.Current.Resolve<IUserContextProvider>();

    private static int _pageSize = 100;
    private int _selectedPageIndex;
    private int _selectedKindIndex;
    private DateTimeOffset _selectedMonth = DateTime.Today;

    private readonly ObservableCollection<JournalRecordKindDto> _kinds = new()
    {
        new JournalRecordKindDto { Id = -1, Name = Resources.JOURNAL__ALL_KINDS }
    };
    private readonly ObservableCollection<int> _pageList = new()
    {
        1
    };
    private IReadOnlyList<JournalRecordInfo> _records = new List<JournalRecordInfo>();
    
    public JournalPageModel(IScreen hostScreen) : base(hostScreen)
    {
        this.WhenNavigatedToObservable()
            .InvokeCommand(ReactiveCommand.CreateFromTask(async () =>
            {
                if (!await TryLoadKindsAsync())
                {
                    return;
                }

                this.WhenAnyValue(
                        vm => vm.SelectedPageIndex,
                        vm => vm.SelectedMonth,
                        vm => vm.SelectedKindIndex)
                    .Select(x => x.Item1)
                    .InvokeCommand(ReactiveCommand.CreateFromTask<int>(LoadRecordsAsync));
            }));
    }

    public IReadOnlyList<int> PageList => _pageList;

    public int SelectedPageIndex
    {
        get => _selectedPageIndex;
        set => this.RaiseAndSetIfChanged(ref _selectedPageIndex, value);
    }

    public DateTimeOffset SelectedMonth
    {
        get => _selectedMonth;
        set => this.RaiseAndSetIfChanged(ref _selectedMonth, value);
    }

    public IReadOnlyList<JournalRecordKindDto> Kinds => _kinds;

    public int SelectedKindIndex
    {
        get => _selectedKindIndex;
        set => this.RaiseAndSetIfChanged(ref _selectedKindIndex, value);
    }

    public IReadOnlyList<JournalRecordInfo> Records
    {
        get => _records;
        set => this.RaiseAndSetIfChanged(ref _records, value);
    }

    /// <inheritdoc />
    public override async ValueTask RefreshAsync()
    {
        if (_userContextProvider.Current.UserId is null)
        {
            await new AuthPageModel(HostScreen).TryNavigateAsync();
            return;
        }

        await LoadRecordsAsync(SelectedPageIndex);
    }

    private async Task<bool> TryLoadKindsAsync()
    {
        using var preloader = Locator.Current.Resolve<AppLoading>().General.Begin();

        var result = await _historyService.GetKindsAsync();
        if (result.Bad)
        {
            return false;
        }

        foreach (var kind in result.Data!.OrderBy(info => info.Name))
        {
            _kinds.Add(kind);
        }

        return true;
    }

    private async Task LoadRecordsAsync(int pageIndex)
    {
        using var preloader = Locator.Current.Resolve<AppLoading>().General.Begin();

        if (pageIndex < 0) return;

        var result = await _historyService.GetListAsync(_pageSize, pageIndex, _selectedMonth.Date, SelectedKindIndex > 0
            ? new[] { Kinds[SelectedKindIndex].Id }
            : null);

        if (result.Bad) return;

        _pageSize = result.Data!.PageSize;

        var pageCount = Math.Max((int)Math.Ceiling((double)result.Data.Total / _pageSize), 1);

        while (_pageList.Count > pageCount)
        {
            _pageList.RemoveAt(_pageList.Count - 1);
        }

        while (_pageList.Count < pageCount)
        {
            _pageList.Add(_pageList.Count + 1);
        }

        SelectedPageIndex = result.Data.PageIndex;

        Records = result.Data.List.Select(rec => new JournalRecordInfo(rec)).ToList();
    }
}