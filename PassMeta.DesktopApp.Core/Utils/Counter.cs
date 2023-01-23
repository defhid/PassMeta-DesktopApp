using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using PassMeta.DesktopApp.Common.Abstractions.Services.Logging;
using PassMeta.DesktopApp.Common.Abstractions.Utils;
using PassMeta.DesktopApp.Common.Abstractions.Utils.FileRepository;
using PassMeta.DesktopApp.Core.Services.Extensions;

namespace PassMeta.DesktopApp.Core.Utils;

/// <inheritdoc />
public class Counter : ICounter
{
    private const string StorageFileName = AppConfig.CounterStorageFileName;

    private readonly SemaphoreSlim _semaphore = new(1);
    private readonly IFileRepository _repository;
    private readonly ILogService _logger;
    private Dictionary<string, long>? _dict;

    /// <summary></summary>
    public Counter(IFileRepository repository, ILogService logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<long> GetNextValueAsync(string name, long gt = 0, CancellationToken cancellationToken = default)
    {
        await _semaphore.WaitAsync(cancellationToken);

        long curr = 0;
        long next;
        try
        {
            _dict ??= await LoadAsync(cancellationToken);
            _dict.TryGetValue(name, out curr);

            _dict[name] = next = Math.Max(curr, gt) + 1;
            await FlushAsync(cancellationToken);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.Error(ex, $"Getting the next value of counter '{name}' failed");

            if (_dict != null)
            {
                _dict[name] = curr;
            }

            throw;
        }
        finally
        {
            _semaphore.Release();
        }

        return next;
    }

    private async ValueTask<Dictionary<string, long>> LoadAsync(CancellationToken cancellationToken)
    {
        Dictionary<string, long>? dict = null;

        if (await _repository.ExistsAsync(StorageFileName, cancellationToken))
        {
            try
            {
                var dictBytes = await _repository.ReadAllBytesAsync(StorageFileName, cancellationToken);

                dict = JsonSerializer.Deserialize<Dictionary<string, long>>(dictBytes);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _logger.Error(ex, "Invalid counter data, reset");
            }
        }

        return dict ?? new Dictionary<string, long>(1);
    }

    private async ValueTask FlushAsync(CancellationToken cancellationToken)
    {
        var dictBytes = JsonSerializer.SerializeToUtf8Bytes(_dict ?? new Dictionary<string, long>());

        await _repository.WriteAllBytesAsync(StorageFileName, dictBytes, cancellationToken);
    }
}