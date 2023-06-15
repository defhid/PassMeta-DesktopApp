using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using PassMeta.DesktopApp.Common;
using PassMeta.DesktopApp.Common.Abstractions.Utils.Logging;
using PassMeta.DesktopApp.Common.Extensions;
using PassMeta.DesktopApp.Common.Models.App;
using PassMeta.DesktopApp.Ui.Models.ViewModels.Base;
using PassMeta.DesktopApp.Ui.Models.ViewModels.Pages.LogsPage.Extra;
using ReactiveUI;
using Splat;

namespace PassMeta.DesktopApp.Ui.Models.ViewModels.Pages.LogsPage;

/// <summary>
/// Logs page ViewModel.
/// </summary>
public class LogsPageModel : PageViewModel
{
    private readonly ILogsManager _logsManager = Locator.Current.Resolve<ILogsManager>();

    private DateTimeOffset _fromDate = DateTime.Now.Date.AddDays(-InitIntervalDays);
    private DateTimeOffset _toDate = DateTime.Now.Date;
    private IReadOnlyList<LogInfo> _logs = new List<LogInfo>();
    private int _selectedLogIndex = -1;
    private string? _foundText;

    private const int InitIntervalDays = 3;
    private const int MaxIntervalDays = 60;

    /// <summary></summary>
    public DateTimeOffset FromDate
    {
        get => _fromDate;
        set => this.RaiseAndSetIfChanged(ref _fromDate, value);
    }

    /// <summary></summary>
    public DateTimeOffset ToDate
    {
        get => _toDate;
        set => this.RaiseAndSetIfChanged(ref _toDate, value);
    }

    /// <summary></summary>
    public IReadOnlyList<LogInfo> Logs
    {
        get => _logs;
        set => this.RaiseAndSetIfChanged(ref _logs, value);
    }

    /// <summary></summary>
    public int SelectedLogIndex
    {
        get => _selectedLogIndex;
        set => this.RaiseAndSetIfChanged(ref _selectedLogIndex, value);
    }

    /// <summary></summary>
    public IObservable<LogInfo> SelectedLog { get; }

    /// <summary></summary>
    public string? FoundText
    {
        get => _foundText;
        set => this.RaiseAndSetIfChanged(ref _foundText, value);
    }

    /// <summary></summary>
    public LogsPageModel(IScreen hostScreen) : base(hostScreen)
    {
        var loadCommand = ReactiveCommand.Create<(DateTimeOffset, DateTimeOffset)>(LoadLogs);

        SelectedLog = this.WhenAnyValue(vm => vm.SelectedLogIndex)
            .Select(index => index < 0 ? new LogInfo(null) : _logs[index]);

        this.WhenAnyValue(vm => vm.FromDate, vm => vm.ToDate)
            .Skip(1)
            .InvokeCommand(loadCommand);

        this.WhenNavigatedToObservable()
            .Select(_ => (FromDate, ToDate)).InvokeCommand(loadCommand);
    }

    /// <inheritdoc />
    public override ValueTask RefreshAsync()
    {
        LoadLogs((FromDate, ToDate));
        return ValueTask.CompletedTask;
    }

    private void LoadLogs((DateTimeOffset from, DateTimeOffset to) period)
    {
        using var preloader = Locator.Current.Resolve<AppLoading>().General.Begin();

        var (fromDate, toDate) = (period.from.Date, period.to.Date);

        if ((toDate - fromDate).Days > MaxIntervalDays || fromDate > toDate)
        {
            FoundText = Resources.LOGS__INCORRECT_PERIOD_LABEL;
            return;
        }

        var logs = _logsManager.Read(fromDate, toDate);
        logs.Reverse();

        Logs = logs.Select(l => new LogInfo(l)).ToList();
        FoundText = Logs.Any() ? null : Resources.LOGS__NOT_FOUND_LABEL;
    }
}