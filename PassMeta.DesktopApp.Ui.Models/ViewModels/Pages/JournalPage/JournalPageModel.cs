using System;
using System.Collections.Generic;
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

    private static int _pageSize = 50;
    private int _selectedPageIndex;
    private int _selectedKindIndex;
    private DateTimeOffset _selectedMonth = DateTime.Today;

    private IReadOnlyList<JournalRecordKindDto> _kinds = new List<JournalRecordKindDto>
    {
        new() { Id = -1, Name = Resources.JOURNAL__ALL_KINDS }
    };
    private IReadOnlyList<int> _pageList = new List<int> { 1 };
    private IReadOnlyList<JournalRecordInfo> _records = new List<JournalRecordInfo>();
    
    /// <summary></summary>
    public JournalPageModel(IScreen hostScreen) : base(hostScreen)
    {
        this.WhenNavigatedToObservable()
            .InvokeCommand(ReactiveCommand.CreateFromTask(async () =>
            {
                await LoadKindsAsync();

                await LoadRecordsAsync(SelectedPageIndex);

                this.WhenAnyValue(
                        vm => vm.SelectedPageIndex,
                        vm => vm.SelectedMonth,
                        vm => vm.SelectedKindIndex)
                    .Select(x => x.Item1)
                    .InvokeCommand(ReactiveCommand.CreateFromTask<int>(LoadRecordsAsync));
            }));
    }

    /// <summary></summary>
    [Obsolete("PREVIEW constructor")]
    public JournalPageModel() : base(null!)
    {
    }

    /// <summary></summary>
    public IReadOnlyList<int> PageList
    {
        get => _pageList;
        set => this.RaiseAndSetIfChanged(ref _pageList, value);
    }

    /// <summary></summary>
    public int SelectedPageIndex
    {
        get => _selectedPageIndex;
        set => this.RaiseAndSetIfChanged(ref _selectedPageIndex, value);
    }

    /// <summary></summary>
    public DateTimeOffset SelectedMonth
    {
        get => _selectedMonth;
        set => this.RaiseAndSetIfChanged(ref _selectedMonth, value);
    }

    /// <summary></summary>
    public IReadOnlyList<JournalRecordKindDto> Kinds
    {
        get => _kinds;
        private set => this.RaiseAndSetIfChanged(ref _kinds, value);
    }

    /// <summary></summary>
    public int SelectedKindIndex
    {
        get => _selectedKindIndex;
        set => this.RaiseAndSetIfChanged(ref _selectedKindIndex, value);
    }

    /// <summary></summary>
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

    private async Task LoadKindsAsync()
    {
        using var preloader = Locator.Current.Resolve<AppLoading>().General.Begin();

        var result = await _historyService.GetKindsAsync();
        if (result.Ok)
        {
            Kinds = Kinds
                .Concat(result.Data!.OrderBy(info => info.Name))
                .ToList();
        }
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

        var pageList = Enumerable.Range(1, (int)Math.Ceiling((double)result.Data.Total / _pageSize)).ToList();
        if (!pageList.Any())
        {
            pageList.Add(1);
        }

        PageList = pageList;
        SelectedPageIndex = result.Data.PageIndex;

        Records = result.Data.List.Select(rec => new JournalRecordInfo(rec)).ToList();
    }
}