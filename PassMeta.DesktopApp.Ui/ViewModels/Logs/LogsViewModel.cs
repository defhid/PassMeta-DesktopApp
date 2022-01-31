namespace PassMeta.DesktopApp.Ui.ViewModels.Logs
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reactive.Linq;
    using System.Threading.Tasks;
    using Base;
    using Common;
    using Common.Interfaces.Services;
    using Core;
    using Models;
    using ReactiveUI;
    using Views.Main;

    public class LogsViewModel : ViewModelPage
    {
        private readonly IDialogService _dialogService = EnvironmentContainer.Resolve<IDialogService>();
        private readonly ILogService _logger = EnvironmentContainer.Resolve<ILogService>();

        private const int InitIntervalDays = 7;
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
        
        public IObservable<bool> AnyFound { get; }

        public LogsViewModel(IScreen hostScreen) : base(hostScreen)
        {
            LogInfo.RefreshStatics();
            
            var loadCommand = ReactiveCommand.Create<(DateTimeOffset, DateTimeOffset)>(LoadLogs);

            SelectedLog = this.WhenAnyValue(vm => vm.SelectedLogIndex)
                .Select(index => index < 0 ? new LogInfo(null) : _logs[index]);

            AnyFound = this.WhenAnyValue(vm => vm.Logs)
                .Select(logs => logs.Any());
            
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
            using var preloader = MainWindow.Current!.StartPreloader();

            var (fromDate, toDate) = (period.from.Date, period.to.Date);
            
            if ((toDate - fromDate).Days > MaxIntervalDays || fromDate > toDate)
            {
                _dialogService.ShowFailure(Resources.LOGS__INCORRECT_PERIOD_ERR);
                return;
            }

            var logs = _logger.ReadLogs(fromDate, toDate);
            logs.Reverse();

            Logs = logs.Select(l => new LogInfo(l)).ToList();
        }
    }
}