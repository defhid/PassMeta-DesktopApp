using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using PassMeta.DesktopApp.Common.Abstractions;
using PassMeta.DesktopApp.Common.Abstractions.Services;
using PassMeta.DesktopApp.Common.Abstractions.Utils.PassMetaClient;
using PassMeta.DesktopApp.Common.Extensions;
using PassMeta.DesktopApp.Common.Models;
using PassMeta.DesktopApp.Common.Models.Dto.Response;

namespace PassMeta.DesktopApp.Core.Services;

/// <inheritdoc />
public class HistoryService : IHistoryService
{
    private readonly IPassMetaClient _pmClient;

    /// <summary></summary>
    public HistoryService(IPassMetaClient pmClient)
    {
        _pmClient = pmClient;
    }

    /// <inheritdoc />
    public async Task<IResult<PageResult<JournalRecordDto>>> GetListAsync(
        int pageSize,
        int pageIndex,
        DateTime month,
        ICollection<int>? selectedKinds,
        CancellationToken cancellationToken = default)
    {
        var url = PassMetaApi.History.GetList(month, pageSize, pageIndex, selectedKinds);

        var response = await _pmClient.Begin(url)
            .WithBadHandling()
            .ExecuteAsync<PageResult<JournalRecordDto>>(cancellationToken);
        
        return Result.FromResponse(response);
    }

    /// <inheritdoc />
    public async Task<IResult<List<JournalRecordKindDto>>> GetKindsAsync(CancellationToken cancellationToken = default)
    {
        var response = await _pmClient.Begin(PassMetaApi.History.GetKinds())
            .WithBadHandling()
            .ExecuteAsync<ListResult<JournalRecordKindDto>>(cancellationToken);

        return response.Success
            ? Result.Success(response.Data!.List)
            : Result.Failure<List<JournalRecordKindDto>>(response.GetFullMessage());
    }
}