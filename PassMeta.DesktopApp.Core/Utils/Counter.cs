using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using PassMeta.DesktopApp.Common.Abstractions.Services.Logging;
using PassMeta.DesktopApp.Common.Abstractions.Utils;
using PassMeta.DesktopApp.Core.Services.Extensions;

namespace PassMeta.DesktopApp.Core.Utils;

/// <inheritdoc />
public class Counter : ICounter
{
    private readonly SemaphoreSlim _semaphore = new(1);
    private readonly ILogService _logger;
    private Dictionary<string, long>? _dict;

    /// <summary></summary>
    public Counter(ILogService logger)
    {
        _logger = logger;
    }
    
    /// <inheritdoc />
    public async Task<long> GetNextValueAsync(string name)
    {
        await _semaphore.WaitAsync();

        _dict ??= await LoadAsync();
        _dict.TryGetValue(name, out var curr);

        long next;
        try
        {
            _dict[name] = next = curr + 1;
            await FlushAsync();
        }
        catch (Exception ex)
        {
            _logger.Error(ex, $"Getting the next value of counter '{name}' failed");
            _dict[name] = curr;
            throw;
        }
        finally
        {
            _semaphore.Release();
        }

        return next;
    }

    private async Task<Dictionary<string, long>> LoadAsync()

    private async Task FlushAsync()
}