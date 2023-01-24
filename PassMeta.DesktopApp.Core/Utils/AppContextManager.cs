using System;
using System.IO;
using System.Reactive.Subjects;
using System.Text.Json;
using System.Threading.Tasks;
using PassMeta.DesktopApp.Common.Abstractions.AppContext;
using PassMeta.DesktopApp.Common.Abstractions.Services.Logging;
using PassMeta.DesktopApp.Common.Models.Dto.Internal;
using PassMeta.DesktopApp.Common.Models.Dto.Response;
using PassMeta.DesktopApp.Common.Models.Entities.Internal;
using PassMeta.DesktopApp.Core.Services.Extensions;

namespace PassMeta.DesktopApp.Core.Utils;

/// <summary>
/// <see cref="IAppContext"/> manager.
/// </summary>
/// <remarks>Singleton.</remarks>
public sealed class AppContextManager : IAppContextManager, IDisposable
{
    private readonly BehaviorSubject<AppContextModel> _currentSubject = new(new AppContextModel(new AppContextDto()));
    private readonly ILogService _logger;

    /// <summary></summary>
    public AppContextManager(ILogService logger)
    {
        _logger = logger;
    }

    /// <inheritdoc />
    public IAppContext Current => _currentSubject.Value;

    /// <inheritdoc />
    public IObservable<IAppContext> CurrentObservable => _currentSubject;

    /// <inheritdoc />
    public async Task LoadAsync()
    {
        AppContextDto? data = null;

        if (File.Exists(AppConfig.ContextFilePath))
        {
            try
            {
                data = JsonSerializer.Deserialize<AppContextDto>(
                    await File.ReadAllBytesAsync(AppConfig.ContextFilePath));
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Context file reading failed");
            }
        }

        var context = new AppContextModel(data ?? new AppContextDto());
        SetCurrent(context);

        if (data is null)
        {
            _ = await SaveToFileAsync(context.ToDto());
        }
    }

    /// <inheritdoc />
    public async Task RefreshFromAsync(PassMetaInfoDto passMetaInfoDto)
    {
        if (Current.User?.Equals(passMetaInfoDto.User) is true &&
            Current.ServerId?.Equals(passMetaInfoDto.AppId) is true &&
            Current.ServerVersion?.Equals(passMetaInfoDto.AppVersion) is true)
        {
            return;
        }

        await ApplyAsync(appContext =>
        {
            appContext.User = passMetaInfoDto.User;
            appContext.ServerId = passMetaInfoDto.AppId;
            appContext.ServerVersion = passMetaInfoDto.AppVersion;
        });
    }

    /// <inheritdoc />
    public async Task ApplyAsync(Action<AppContextModel> setup)
    {
        var copy = _currentSubject.Value.Copy();
        setup(copy);

        if (await SaveToFileAsync(copy.ToDto()))
        {
            SetCurrent(copy);
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
            var path = AppConfig.ContextFilePath;
            var oldPath = AppConfig.ContextFilePath + ".old";

            var creatingNew = !File.Exists(path);
            if (creatingNew)
            {
                _logger.Info("Creating a new context file...");
            }
            else
            {
                File.SetAttributes(path, File.GetAttributes(path) & ~FileAttributes.Hidden);
                if (File.Exists(oldPath))
                {
                    File.SetAttributes(path, File.GetAttributes(oldPath) & ~FileAttributes.Hidden);
                }

                File.Move(path, oldPath, true);
                File.SetAttributes(oldPath, FileAttributes.Hidden);
            }

            await File.WriteAllBytesAsync(path, JsonSerializer.SerializeToUtf8Bytes(dto));

            File.SetAttributes(path, FileAttributes.Hidden);

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