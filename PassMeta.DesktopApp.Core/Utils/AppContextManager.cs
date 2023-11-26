using System;
using System.Reactive.Subjects;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using PassMeta.DesktopApp.Common.Abstractions;
using PassMeta.DesktopApp.Common.Abstractions.App;
using PassMeta.DesktopApp.Common.Abstractions.Utils.FileRepository;
using PassMeta.DesktopApp.Common.Abstractions.Utils.Logging;
using PassMeta.DesktopApp.Common.Extensions;
using PassMeta.DesktopApp.Common.Models;
using PassMeta.DesktopApp.Common.Models.Dto.Internal;
using PassMeta.DesktopApp.Common.Models.Dto.Response;
using PassMeta.DesktopApp.Common.Models.Internal;

namespace PassMeta.DesktopApp.Core.Utils;

/// <summary>
/// <see cref="IAppContext"/> manager.
/// </summary>
/// <remarks>Singleton.</remarks>
public sealed class AppContextManager : IAppContextManager, IUserContextProvider, IDisposable
{
    private const string CurrentContextFileName = ".context";
    private const string PreviousContextFileName = ".context.old";

    private readonly BehaviorSubject<AppContextModel> _currAppSubject = new(new AppContextModel(new AppContextDto()));
    private readonly BehaviorSubject<UserContextModel> _currUserSubject = new(new UserContextModel(null, null));
    private readonly IFileRepository _repository;
    private readonly ILogsWriter _logger;
    private readonly SemaphoreSlim _semaphore = new(1);

    /// <summary></summary>
    public AppContextManager(ILogsWriter logger, IFileRepository fileRepository)
    {
        _logger = logger;
        _repository = fileRepository;
    }

    /// <inheritdoc />
    IAppContext IAppContextProvider.Current => _currAppSubject.Value;

    /// <inheritdoc />
    IObservable<IAppContext> IAppContextProvider.CurrentObservable => _currAppSubject;

    /// <inheritdoc />
    IUserContext IUserContextProvider.Current => _currUserSubject.Value;

    /// <inheritdoc />
    IObservable<IUserContext> IUserContextProvider.CurrentObservable => _currUserSubject;

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
        if (_currAppSubject.Value.User?.Equals(passMetaInfoDto.User) is true &&
            _currAppSubject.Value.ServerId?.Equals(passMetaInfoDto.AppId) is true &&
            _currAppSubject.Value.ServerVersion?.Equals(passMetaInfoDto.AppVersion) is true)
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
        var copy = _currAppSubject.Value.Copy();
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
        _currAppSubject.Dispose();
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

    private void SetCurrent(AppContextModel appContext)
    {
        _currAppSubject.OnNext(appContext);

        var userContext = new UserContextModel(appContext.User?.Id, appContext.ServerId);
        if (userContext.UniqueId != _currUserSubject.Value.UniqueId)
        {
            _currUserSubject.OnNext(userContext);
        }
    }
}