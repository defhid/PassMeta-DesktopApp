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
using PassMeta.DesktopApp.Ui.Models.Base;
using PassMeta.DesktopApp.Ui.Models.Journal.Models;
using PassMeta.DesktopApp.Ui.Models.Logs.Models;
using ReactiveUI;
using Splat;

namespace PassMeta.DesktopApp.Ui.Models.Journal;

public class JournalModel : PageViewModel
{
    private static int _pageSize = 50;
        
    #region Filters

    private List<int> _pageList = new() { 1 };
    public List<int> PageList
    {
        get => _pageList;
        set => this.RaiseAndSetIfChanged(ref _pageList, value);
    }
        
    private int _selectedPageIndex;
    public int SelectedPageIndex
    {
        get => _selectedPageIndex;
        set => this.RaiseAndSetIfChanged(ref _selectedPageIndex, value);
    }
        
    private DateTimeOffset _selectedMonth = DateTime.Today;
    public DateTimeOffset SelectedMonth
    {
        get => _selectedMonth;
        set => this.RaiseAndSetIfChanged(ref _selectedMonth, value);
    }

    private List<JournalRecordKindDto> _kinds = new();
    public List<JournalRecordKindDto> Kinds
    {
        get => _kinds; 
        private set => this.RaiseAndSetIfChanged(ref _kinds, value);
    }

    private JournalRecordKindDto? _selectedKind;
    public JournalRecordKindDto? SelectedKind
    {
        get => _selectedKind;
        set => this.RaiseAndSetIfChanged(ref _selectedKind, value);
    }
        
    #endregion

    private IReadOnlyList<JournalRecordInfo> _records = new List<JournalRecordInfo>();
    public IReadOnlyList<JournalRecordInfo> Records
    {
        get => _records;
        set => this.RaiseAndSetIfChanged(ref _records, value);
    }

    private readonly IHistoryService _historyService = Locator.Current.Resolve<IHistoryService>();
    private readonly IUserContextProvider _userContextProvider = Locator.Current.Resolve<IUserContextProvider>();

    public JournalModel(IScreen hostScreen) : base(hostScreen)
    {
        LogInfo.RefreshStatics();

        const int skipInitChanges = 3;
            
        this.WhenAnyValue(
                vm => vm.SelectedPageIndex, 
                vm => vm.SelectedMonth,
                vm => vm.SelectedKind)
            .Select(x => x.Item1)
            .Skip(skipInitChanges)
            .InvokeCommand(ReactiveCommand.CreateFromTask<int>(LoadRecordsAsync));

        this.WhenNavigatedToObservable()
            .InvokeCommand(ReactiveCommand.CreateFromTask(InitLoadAsync));
    }
        
    public override void TryNavigate()
    {
        if (_userContextProvider.Current.UserId is null)
        {
            TryNavigateTo<AuthRequiredModel>();
        }
        else
        {
            base.TryNavigate();
        }
    }

    public override Task RefreshAsync() => LoadRecordsAsync(SelectedPageIndex);

    private async Task LoadRecordsAsync(int pageIndex)
    {
        using var preloader = Locator.Current.Resolve<AppLoading>().General.Begin();

        if (pageIndex < 0) return;

        var result = await _historyService.GetListAsync(_pageSize, pageIndex, _selectedMonth.Date, SelectedKind?.Id > 0
            ? new[] { SelectedKind.Id }
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

    private async Task InitLoadAsync()
    {
        var defaultKind = new JournalRecordKindDto { Id = -1, Name = Resources.JOURNAL__ALL_KINDS};

        if (!Kinds.Any())
        {
            using var preloader = Locator.Current.Resolve<AppLoading>().General.Begin();
                
            var result = await _historyService.GetKindsAsync();
            if (result.Bad) return;

            Kinds = result.Data!
                .OrderBy(info => info.Name)
                .Prepend(defaultKind)
                .ToList();
        }

        SelectedKind = defaultKind;

        await LoadRecordsAsync(SelectedPageIndex);
    }
}