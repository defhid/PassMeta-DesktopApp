namespace PassMeta.DesktopApp.Ui.ViewModels.Journal
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reactive.Linq;
    using System.Threading.Tasks;
    using Base;
    using Common;
    using Common.Models.Dto.Response;
    using Common.Models.Entities;
    using Core.Utils;
    using Logs.Models;
    using Models;
    using ReactiveUI;
    using Views.Main;

    using AppContext = Core.AppContext;

    public class JournalViewModel : PageViewModel
    {
        private static int _pageLimit = 50;
        
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

        private List<JournalRecordKind> _kinds = new();
        public List<JournalRecordKind> Kinds
        {
            get => _kinds; 
            private set => this.RaiseAndSetIfChanged(ref _kinds, value);
        }

        private JournalRecordKind? _selectedKind;
        public JournalRecordKind? SelectedKind
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
            if (AppContext.Current.User is null)
            {
                FakeNavigated();
                TryNavigateTo<AuthRequiredViewModel>(typeof(JournalViewModel));
            }
            else
            {
                base.TryNavigate();
            }
        }

        public override Task RefreshAsync() => LoadRecordsAsync(SelectedPageIndex);

        private async Task LoadRecordsAsync(int pageIndex)
        {
            using var preloader = MainWindow.Current!.StartPreloader();

            if (pageIndex < 0) return;

            var kind = SelectedKind?.Id > 0 ? ("&kind=" + SelectedKind.Id) : string.Empty;
            var offset = "offset=" + pageIndex * _pageLimit;
            var limit = "limit=" + _pageLimit;
            var month = "month=" + _selectedMonth.Date.ToString("O");

            var response = await PassMetaApi.GetAsync<PageResult<JournalRecord>>($"history?{month}&{limit}&{offset}{kind}", true);
            if (response?.Success is not true) return;

            _pageLimit = response.Data!.Limit;

            var pageList = Enumerable.Range(1, (int)Math.Ceiling((double)response.Data.Total / _pageLimit)).ToList();
            if (!pageList.Any())
            {
                pageList.Add(1);
            }

            PageList = pageList;
            SelectedPageIndex = response.Data.Offset / _pageLimit;

            Records = response.Data.List.Select(rec => new JournalRecordInfo(rec)).ToList();
        }

        private async Task InitLoadAsync()
        {
            var defaultKind = new JournalRecordKind { Id = -1, Name = Resources.JOURNAL__ALL_KINDS};

            if (!Kinds.Any())
            {
                using var preloader = MainWindow.Current!.StartPreloader();
                
                var kindsResponse = await PassMetaApi.GetAsync<List<JournalRecordKind>>("history/kinds", true);

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
}