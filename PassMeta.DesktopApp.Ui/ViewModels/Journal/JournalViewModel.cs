namespace PassMeta.DesktopApp.Ui.ViewModels.Journal
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reactive.Linq;
    using System.Threading.Tasks;
    using Base;
    using Common.Interfaces.Mapping;
    using Common.Models.Dto.Response;
    using Common.Models.Entities;
    using Common.Utils.Mapping;
    using Core.Utils;
    using Core.Utils.Mapping;
    using Logs.Models;
    using Models;
    using ReactiveUI;
    using Views.Main;

    public class JournalViewModel : ViewModelPage
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

        private List<JournalRecordKindInfo> _kinds = new();
        public List<JournalRecordKindInfo> Kinds
        {
            get => _kinds; 
            private set => this.RaiseAndSetIfChanged(ref _kinds, value);
        }

        private JournalRecordKindInfo? _selectedKind;
        public JournalRecordKindInfo? SelectedKind
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
        
        private static IMapper<int, string>? _kindMapper;

        public JournalViewModel(IScreen hostScreen) : base(hostScreen)
        {
            LogInfo.RefreshStatics();
            
            this.WhenAnyValue(vm => vm.SelectedPageIndex, vm => vm.SelectedKind)
                .Select(x => x.Item1)
                .Skip(4)
                .InvokeCommand(ReactiveCommand.CreateFromTask<int>(LoadRecordsAsync));

            this.WhenNavigatedToObservable()
                .InvokeCommand(ReactiveCommand.CreateFromTask(InitLoadAsync));
        }

        public override Task RefreshAsync() => LoadRecordsAsync(SelectedPageIndex);

        private async Task LoadRecordsAsync(int pageIndex)
        {
            using var preloader = MainWindow.Current!.StartPreloader();

            if (pageIndex < 0) return;

            var kind = SelectedKind?.Id > 0 ? ("&kind=" + SelectedKind.Id) : string.Empty;
            var limit = "limit=" + _pageLimit;
            var offset = "offset=" + pageIndex * _pageLimit;

            var response = await PassMetaApi.GetAsync<PageResult<JournalRecord>>($"history?{limit}&{offset}{kind}", true);
            if (response?.Success is not true) return;

            _pageLimit = response.Data!.Limit;

            var pageList = Enumerable.Range(1, (int)Math.Ceiling((double)response.Data.Total / _pageLimit)).ToList();
            if (!pageList.Any())
            {
                pageList.Add(1);
            }

            PageList = pageList;
            SelectedPageIndex = response.Data.Offset / _pageLimit;

            Records = response.Data.List.Select(rec => new JournalRecordInfo(rec, _kindMapper!)).ToList();
        }

        private async Task InitLoadAsync()
        {
            if (_kindMapper is null)
            {
                using var preloader = MainWindow.Current!.StartPreloader();
                
                var kindsResponse = await PassMetaApi.GetAsync<List<JournalRecordKind>>("history/kinds", true);

                if (kindsResponse?.Success is not true) return;

                _kindMapper = new SimpleMapper<int, string>(
                    kindsResponse.Data!.Select(kind => new MapToTranslate<int>(kind.Id, kind.NamePack)));
            }

            var defaultKind = new JournalRecordKindInfo(-1, "-");
            
            Kinds = _kindMapper.GetMappings()
                .Select(map => new JournalRecordKindInfo(map.From, map.To))
                .OrderBy(info => info.Name)
                .Prepend(defaultKind)
                .ToList();

            SelectedKind = defaultKind;
            SelectedPageIndex = 0;

            await LoadRecordsAsync(0);
        }
    }
}