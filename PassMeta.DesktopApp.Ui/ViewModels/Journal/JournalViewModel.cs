using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using ReactiveUI;
    
using PassMeta.DesktopApp.Common;
using PassMeta.DesktopApp.Common.Abstractions.AppContext;
using PassMeta.DesktopApp.Common.Abstractions.Utils.PassMetaClient;
using PassMeta.DesktopApp.Common.Models.Dto.Response;
    
using PassMeta.DesktopApp.Core;
using PassMeta.DesktopApp.Core.Extensions;
using PassMeta.DesktopApp.Ui.App;

using PassMeta.DesktopApp.Ui.ViewModels.Base;
using PassMeta.DesktopApp.Ui.ViewModels.Logs.Models;
using PassMeta.DesktopApp.Ui.ViewModels.Journal.Models;
using Splat;

namespace PassMeta.DesktopApp.Ui.ViewModels.Journal;

public class JournalViewModel : PageViewModel
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

    private readonly IPassMetaClient _pmClient = Locator.Current.Resolve<IPassMetaClient>();
    private readonly IUserContextProvider _userContextProvider = Locator.Current.Resolve<IUserContextProvider>();

    public JournalViewModel(IScreen hostScreen) : base(hostScreen)
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
            TryNavigateTo<AuthRequiredViewModel>();
        }
        else
        {
            base.TryNavigate();
        }
    }

    public override Task RefreshAsync() => LoadRecordsAsync(SelectedPageIndex);

    private async Task LoadRecordsAsync(int pageIndex)
    {
        using var preloader = AppLoading.General.Begin();

        if (pageIndex < 0) return;

        var url = PassMetaApi.History.GetList(_selectedMonth.Date, _pageSize, pageIndex, SelectedKind?.Id > 0 
            ? new[] { SelectedKind.Id }
            : null);

        var response = await _pmClient.Begin(url)
            .WithBadHandling()
            .ExecuteAsync<PageResult<JournalRecordDto>>();

        if (response?.Success is not true) return;

        _pageSize = response.Data!.PageSize;

        var pageList = Enumerable.Range(1, (int)Math.Ceiling((double)response.Data.Total / _pageSize)).ToList();
        if (!pageList.Any())
        {
            pageList.Add(1);
        }

        PageList = pageList;
        SelectedPageIndex = response.Data.PageIndex;

        Records = response.Data.List.Select(rec => new JournalRecordInfo(rec)).ToList();
    }

    private async Task InitLoadAsync()
    {
        var defaultKind = new JournalRecordKindDto { Id = -1, Name = Resources.JOURNAL__ALL_KINDS};

        if (!Kinds.Any())
        {
            using var preloader = AppLoading.General.Begin();
                
            var kindsResponse = await _pmClient.Begin(PassMetaApi.History.GetKinds())
                .WithBadHandling()
                .ExecuteAsync<List<JournalRecordKindDto>>();

            if (kindsResponse?.Success is not true) return;

            Kinds = kindsResponse.Data!
                .OrderBy(info => info.Name)
                .Prepend(defaultKind)
                .ToList();
        }

        SelectedKind = defaultKind;

        await LoadRecordsAsync(SelectedPageIndex);
    }
}