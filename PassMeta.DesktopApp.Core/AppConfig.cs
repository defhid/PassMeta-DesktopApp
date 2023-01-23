using System;
using System.IO;
using System.Reactive.Subjects;
using System.Reflection;
using System.Text.Json;
using System.Threading.Tasks;

using PassMeta.DesktopApp.Common;
using PassMeta.DesktopApp.Common.Abstractions;
using PassMeta.DesktopApp.Common.Abstractions.Services.Logging;
using PassMeta.DesktopApp.Common.Models;
using PassMeta.DesktopApp.Common.Models.Dto.Internal;
using PassMeta.DesktopApp.Core.Models;
using PassMeta.DesktopApp.Core.Services.Extensions;

namespace PassMeta.DesktopApp.Core;

/// <summary>
/// Application configuration.
/// </summary>
public static class AppConfig
{
    private static readonly BehaviorSubject<AppConfigModel> CurrentSubject = new(new AppConfigModel(new AppConfigDto()));
    private static ILogService Logger => EnvironmentContainer.Resolve<ILogService>();

    /// <summary>
    /// Current application config.
    /// </summary>
    public static IAppConfig Current => CurrentSubject.Value;

    /// <summary>
    /// Represents <see cref="Current"/>.
    /// </summary>
    public static IObservable<AppConfigModel> CurrentObservable => CurrentSubject;

    #region Consts

    /// <summary>
    /// Application root directory.
    /// </summary>
    public static readonly string RootPath = Path.GetDirectoryName(Assembly.GetAssembly(typeof(AppConfig))!.Location)!;
        
    /// <summary>
    /// Path to application config file.
    /// </summary>
    public static readonly string ConfigFilePath = Path.Combine(RootPath, ".config");
        
    /// <summary>
    /// Path to application context file.
    /// </summary>
    public static readonly string ContextFilePath = Path.Combine(RootPath, ".context");

    /// <summary>
    /// Path to passfiles storage.
    /// </summary>
    public static readonly string PassFilesDirectory = Path.Combine(RootPath, ".passfiles");
        
    /// <summary>
    /// Path to application logs.
    /// </summary>
    public static readonly string LogFilesDirectory = Path.Combine(RootPath, ".logs");

    /// <summary>
    /// File name for counter storage.
    /// </summary>
    public const string CounterStorageFileName = ".counter";

    #endregion

    /// <summary>
    /// Load and set app configuration to <see cref="Current"/>.
    /// </summary>
    public static async Task LoadAndSetCurrentAsync()
    {
        AppConfigDto? data = null;
            
        if (File.Exists(ConfigFilePath))
        {
            try
            {
                await using var stream = File.OpenRead(ConfigFilePath);
                data = JsonSerializer.Deserialize<AppConfigDto>(stream);
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Configuration file reading failed");
            }
        }

        var config = new AppConfigModel(data ?? new AppConfigDto());
        SetCurrent(config);

        if (data is null)
        {
            _ = await SaveToFileAsync(config.ToDto());
        }
    }

    /// <summary>
    /// Edit current config model and flush changes.
    /// </summary>
    public static async Task<IDetailedResult> ApplyAsync(Action<AppConfigModel> setup)
    {
        var copy = CurrentSubject.Value.Copy();
        setup(copy);
        
        var result = await SaveToFileAsync(copy.ToDto());
        if (result.Bad) return result;
        
        SetCurrent(copy);
        return Result.Success();
    }

    private static async Task<IDetailedResult> SaveToFileAsync(AppConfigDto dto)
    {
        try
        {
            FileAttributes attributes = default;
                
            var creatingNew = !File.Exists(ConfigFilePath);
            if (creatingNew)
            {
                Logger.Info("Creating a new configuration file...");
            }
            else
            {
                attributes = File.GetAttributes(ConfigFilePath);
                attributes &= ~FileAttributes.Hidden;
                File.SetAttributes(ConfigFilePath, attributes);
            }

            await File.WriteAllBytesAsync(ConfigFilePath, JsonSerializer.SerializeToUtf8Bytes(dto));

            attributes |= FileAttributes.Hidden;
            File.SetAttributes(ConfigFilePath, attributes);
                
            if (creatingNew)
                Logger.Info("Configuration file created successfully");
                
            return Result.Success();
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Configuration file saving failed");
            return Result.Failure(Resources.APP_CONFIG__SAVE_ERR);
        }
    }

    private static void SetCurrent(AppConfigModel appConfig) => CurrentSubject.OnNext(appConfig);
}