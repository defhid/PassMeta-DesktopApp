using PassMeta.DesktopApp.Common.Abstractions.Utils.Logging;

namespace PassMeta.DesktopApp.Ui.ViewModels.Logs
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reactive.Linq;
    using System.Threading.Tasks;
    using Base;
    using Common;
    using Core;
    using Models;
    using ReactiveUI;

    public class LogsViewModel : PageViewModel
    {
        private readonly ILogsWriter _logger = EnvironmentContainer.Resolve<ILogsWriter>();

        private const int InitIntervalDays = 3;
        private const int MaxIntervalDays = 60;

        private DateTimeOffset _fromDate = DateTime.Now.Date.AddDays(-InitIntervalDays);
        public DateTimeOffset FromDate
        {
            get => _fromDate;
            set => this.RaiseAndSetIfChanged(ref _fromDate, value);
        }
        
        private DateTimeOffset _toDate = DateTime.Now.Date;
        public DateTimeOffset ToDate
        {
            get => _toDate;
            set => this.RaiseAndSetIfChanged(ref _toDate, value);
        }

        private IReadOnlyList<LogInfo> _logs = new List<LogInfo>();
        public IReadOnlyList<LogInfo> Logs
        {
            get => _logs;
            set => this.RaiseAndSetIfChanged(ref _logs, value);
        }

        private int _selectedLogIndex = -1;
        public int SelectedLogIndex
        {
            get => _selectedLogIndex;
            set => this.RaiseAndSetIfChanged(ref _selectedLogIndex, value);
        }
        
        public IObservable<LogInfo> SelectedLog { get; }

        private string? _foundText;
        public string? FoundText
        {
            get => _foundText;
            set => this.RaiseAndSetIfChanged(ref _foundText, value);
        }

        public LogsViewModel(IScreen hostScreen) : base(hostScreen)
        {
            LogInfo.RefreshStatics();
            
            var loadCommand = ReactiveCommand.Create<(DateTimeOffset, DateTimeOffset)>(LoadLogs);

            SelectedLog = this.WhenAnyValue(vm => vm.SelectedLogIndex)
                .Select(index => index < 0 ? new LogInfo(null) : _logs[index]);

            this.WhenAnyValue(vm => vm.FromDate, vm => vm.ToDate)
                .Skip(1)
                .InvokeCommand(loadCommand);

            this.WhenNavigatedToObservable()
                .Select(_ => (FromDate, ToDate)).InvokeCommand(loadCommand);
        }

        public override Task RefreshAsync()
        {
            LoadLogs((FromDate, ToDate));
            return Task.CompletedTask;
        }

        private void LoadLogs((DateTimeOffset from, DateTimeOffset to) period)
        {
            using var preloader = AppLoading.General.Begin();

            var (fromDate, toDate) = (period.from.Date, period.to.Date);
            
            if ((toDate - fromDate).Days > MaxIntervalDays || fromDate > toDate)
            {
                FoundText = Resources.LOGS__INCORRECT_PERIOD_LABEL;
                return;
            }

            var logs = _logger.Read(fromDate, toDate);
            logs.Reverse();

            Logs = logs.Select(l => new LogInfo(l)).ToList();
            FoundText = Logs.Any() ? null : Resources.LOGS__NOT_FOUND_LABEL;
        }
    }
}