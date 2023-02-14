using System;
using System.Reactive.Subjects;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using PassMeta.DesktopApp.Common.Abstractions;
using PassMeta.DesktopApp.Common.Abstractions.AppContext;
using PassMeta.DesktopApp.Common.Abstractions.Utils.FileRepository;
using PassMeta.DesktopApp.Common.Abstractions.Utils.Logging;
using PassMeta.DesktopApp.Common.Models;
using PassMeta.DesktopApp.Common.Models.Dto.Internal;
using PassMeta.DesktopApp.Common.Models.Dto.Response;
using PassMeta.DesktopApp.Common.Models.Entities.Internal;
using PassMeta.DesktopApp.Core.Services.Extensions;

namespace PassMeta.DesktopApp.Core.Utils;

/// <summary>
/// <see cref="IAppContext"/> manager.
/// </summary>
/// <remarks>Singleton.</remarks>
public sealed class AppContextManager : IAppContextManager, IUserContextProvider, IDisposable
{
    private const string CurrentContextFileName = ".context";
    private const string PreviousContextFileName = ".context.old";

    private readonly BehaviorSubject<AppContextModel> _currentSubject = new(new AppContextModel(new AppContextDto()));
    private readonly IFileRepository _repository;
    private readonly ILogsWriter _logger;
    private readonly SemaphoreSlim _semaphore = new(1);

    /// <summary></summary>
    public AppContextManager(ILogsWriter logger, IFileRepositoryFactory fileRepositoryFactory)
    {
        _logger = logger;
        _repository = fileRepositoryFactory.ForSystemFiles();
    }

    /// <inheritdoc />
    IAppContext IAppContextProvider.Current => _currentSubject.Value;

    /// <inheritdoc />
    IObservable<IAppContext> IAppContextProvider.CurrentObservable => _currentSubject;

    /// <inheritdoc />
    IUserContext IUserContextProvider.Current
        => new UserContextModel(_currentSubject.Value.User?.Id, _currentSubject.Value.ServerId);

    /// <inheritdoc />
    public async Task LoadAsync()
    {
        AppContextDto? data = null;

        if (await _repository.ExistsAsync(CurrentContextFileName))
        {
            try
            {
                data = JsonSerializer.Deserialize<AppContextDto>(
                    await _repository.ReadAllBytesAsync(CurrentContextFileName));
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Context file reading failed");
            }
        }

        await _semaphore.WaitAsync();
        try
        {
            var context = new AppContextModel(data ?? new AppContextDto());
            SetCurrent(context);

            if (data is null)
            {
                _ = await SaveToFileAsync(context.ToDto());
            }
        }
        finally
        {
            _semaphore.Release();
        }
    }

    /// <inheritdoc />
    public async Task<IResult> RefreshFromAsync(PassMetaInfoDto passMetaInfoDto)
    {
        if (_currentSubject.Value.User?.Equals(passMetaInfoDto.User) is true &&
            _currentSubject.Value.ServerId?.Equals(passMetaInfoDto.AppId) is true &&
            _currentSubject.Value.ServerVersion?.Equals(passMetaInfoDto.AppVersion) is true)
        {
            return Result.Success();
        }

        return await ApplyAsync(appContext =>
        {
            appContext.User = passMetaInfoDto.User;
            appContext.ServerId = passMetaInfoDto.AppId;
            appContext.ServerVersion = passMetaInfoDto.AppVersion;
        });
    }

    /// <inheritdoc />
    public async Task<IResult> ApplyAsync(Action<AppContextModel> setup)
    {
        var copy = _currentSubject.Value.Copy();
        setup(copy);

        await _semaphore.WaitAsync();
        try
        {
            if (!await SaveToFileAsync(copy.ToDto()))
            {
                return Result.Failure();
            }

            SetCurrent(copy);
            return Result.Success();
        }
        finally
        {
            _semaphore.Release();
        }
    }

    /// <inheritdoc />
    public void Dispose()
    {
        _currentSubject.Dispose();
    }

    private async Task<bool> SaveToFileAsync(AppContextDto dto)
    {
        try
        {
            var creatingNew = !await _repository.ExistsAsync(CurrentContextFileName);
            if (creatingNew)
            {
                _logger.Info("Creating a new context file...");
            }
            else
            {
                if (await _repository.ExistsAsync(PreviousContextFileName))
                {
                    await _repository.DeleteAsync(PreviousContextFileName);
                }

                await _repository.RenameAsync(CurrentContextFileName, PreviousContextFileName);
            }

            await _repository.WriteAllBytesAsync(CurrentContextFileName, JsonSerializer.SerializeToUtf8Bytes(dto));

            _logger.Debug("Context file saved");

            if (creatingNew)
                _logger.Info("Context file created successfully");

            return true;
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Context file saving failed");
            return false;
        }
    }

    private void SetCurrent(AppContextModel appContext) => _currentSubject.OnNext(appContext);
}