namespace PassMeta.DesktopApp.Ui.ViewModels.Journal
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
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
        
        private int _selectedPageIndex = 0;
        public int SelectedPageIndex
        {
            get => _selectedPageIndex;
            set => this.RaiseAndSetIfChanged(ref _selectedPageIndex, value);
        }

        public List<JournalRecordKind> Kinds { get; private set; } = new();
        public ObservableCollection<JournalRecordKind> SelectedKinds { get; } = new();
        
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
            
            this.WhenAnyValue(vm => vm.SelectedPageIndex, vm => vm.SelectedKinds)
                .Skip(1)
                .InvokeCommand(ReactiveCommand.CreateFromTask<(int, IList<JournalRecordKind>)>(LoadRecordsAsync));

            this.WhenNavigatedToObservable()
                .InvokeCommand(ReactiveCommand.CreateFromTask(InitLoadAsync));
        }

        public override Task RefreshAsync()
        {
            return LoadRecordsAsync((SelectedPageIndex, Kinds));
        }

        private async Task LoadRecordsAsync((int pageIndex, IList<JournalRecordKind> kinds) filter)
        {
            using var preloader = MainWindow.Current!.StartPreloader();

            var kind = filter.kinds.Any() ? ("&kind=" + string.Join(",", filter.kinds)) : string.Empty;
            var limit = "limit=" + _pageLimit;
            var offset = "offset=" + filter.pageIndex * _pageLimit;

            var response = await PassMetaApi.GetAsync<PageResult<JournalRecord>>($"/history?{limit}&{offset}{kind}", true);
            if (response?.Success is not true) return;

            _pageLimit = response.Data!.Limit;
            PageList = Enumerable.Range(1, response.Data.Total / _pageLimit).ToList();
            SelectedPageIndex = response.Data.Offset / _pageLimit;

            Records = response.Data.List.Select(rec => new JournalRecordInfo(rec, _kindMapper!)).ToList();
        }

        private async Task InitLoadAsync()
        {
            if (_kindMapper is null)
            {
                using var preloader = MainWindow.Current!.StartPreloader();
                
                var kindsResponse = await PassMetaApi
                    .GetAsync<List<Dictionary<string, object>>>("/history/kinds", true);

                if (kindsResponse?.Success is not true) return;

                _kindMapper = new SimpleMapper<int, string>(kindsResponse.Data!
                    .Select(dict =>
                        new MapToTranslate<int>((int)dict["id"], (IDictionary<string, string>)dict["name"])));
            }
            
            Kinds = _kindMapper.GetMappings()
                .Select(map => new JournalRecordKind(map.From, map.To))
                .ToList();
            
            this.RaisePropertyChanged(nameof(Kinds));

            await LoadRecordsAsync((SelectedPageIndex, SelectedKinds));
        }
    }
}