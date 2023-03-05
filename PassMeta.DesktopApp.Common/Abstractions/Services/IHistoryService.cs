using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using PassMeta.DesktopApp.Common.Models.Dto.Response;

namespace PassMeta.DesktopApp.Common.Abstractions.Services;

/// <summary>
/// Service for working with remote history.
/// </summary>
public interface IHistoryService
{
    /// <summary>
    /// Get paged list of journal records by month and selected month.
    /// </summary>
    Task<IResult<PageResult<JournalRecordDto>>> GetListAsync(
        int pageSize,
        int pageIndex,
        DateTime month,
        ICollection<int>? selectedKinds,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get list of journal record kinds.
    /// </summary>
    Task<IResult<List<JournalRecordKindDto>>> GetKindsAsync(CancellationToken cancellationToken = default);
}