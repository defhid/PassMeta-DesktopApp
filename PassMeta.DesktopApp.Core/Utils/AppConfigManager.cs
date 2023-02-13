using System;
using System.Reactive.Subjects;
using System.Text.Json;
using System.Threading.Tasks;
using PassMeta.DesktopApp.Common;
using PassMeta.DesktopApp.Common.Abstractions;
using PassMeta.DesktopApp.Common.Abstractions.AppConfig;
using PassMeta.DesktopApp.Common.Abstractions.Utils.FileRepository;
using PassMeta.DesktopApp.Common.Abstractions.Utils.Logging;
using PassMeta.DesktopApp.Common.Models;
using PassMeta.DesktopApp.Common.Models.Dto.Internal;
using PassMeta.DesktopApp.Common.Models.Entities.Internal;
using PassMeta.DesktopApp.Core.Services.Extensions;

namespace PassMeta.DesktopApp.Core.Utils;

/// <inheritdoc />
public class AppConfigManager : IAppConfigManager
{
    private const string CurrentConfigFileName = ".config";
    private const string PreviousConfigFileName = ".config.old";

    private readonly BehaviorSubject<AppConfigModel> _currentSubject = new(new AppConfigModel(new AppConfigDto()));
    private readonly IFileRepository _repository;
    private readonly ILogsWriter _logger;

    /// <summary></summary>
    public AppConfigManager(ILogsWriter logger, IFileRepositoryFactory fileRepositoryFactory)
    {
        _logger = logger;
        _repository = fileRepositoryFactory.ForSystemFiles();
    }

    /// <inheritdoc />
    public IAppConfig Current => _currentSubject.Value;

    /// <inheritdoc />
    public IObservable<IAppConfig> CurrentObservable => _currentSubject;

    /// <inheritdoc />
    public async Task LoadAsync()
    {
        AppConfigDto? data = null;

        if (await _repository.ExistsAsync(CurrentConfigFileName))
        {
            try
            {
                data = JsonSerializer.Deserialize<AppConfigDto>(
                    await _repository.ReadAllBytesAsync(CurrentConfigFileName));
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Configuration file reading failed");
            }
        }

        var config = new AppConfigModel(data ?? new AppConfigDto());
        SetCurrent(config);

        if (data is null)
        {
            _ = await SaveToFileAsync(config.ToDto());
        }
    }

    /// <inheritdoc />
    public async Task<IResult> ApplyAsync(Action<AppConfigModel> setup)
    {
        var copy = _currentSubject.Value.Copy();
        setup(copy);

        var result = await SaveToFileAsync(copy.ToDto());
        if (result.Bad) return result;

        SetCurrent(copy);
        return Result.Success();
    }

    private async Task<IDetailedResult> SaveToFileAsync(AppConfigDto dto)
    {
        try
        {
            var creatingNew = !await _repository.ExistsAsync(CurrentConfigFileName);
            if (creatingNew)
            {
                _logger.Info("Creating a new configuration file...");
            }
            else
            {
                if (await _repository.ExistsAsync(PreviousConfigFileName))
                {
                    await _repository.DeleteAsync(PreviousConfigFileName);
                }

                await _repository.RenameAsync(CurrentConfigFileName, PreviousConfigFileName);
            }

            await _repository.WriteAllBytesAsync(CurrentConfigFileName, JsonSerializer.SerializeToUtf8Bytes(dto));

            _logger.Debug("Configuration file saved");

            if (creatingNew)
                _logger.Info("Configuration file created successfully");

            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Configuration file saving failed");
            return Result.Failure(Resources.APP_CONFIG__SAVE_ERR);
        }
    }

    private void SetCurrent(AppConfigModel appConfig) => _currentSubject.OnNext(appConfig);
}