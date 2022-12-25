using System;
using System.IO;
using System.Reactive.Subjects;
using System.Threading.Tasks;
using Newtonsoft.Json;

using PassMeta.DesktopApp.Common.Abstractions;
using PassMeta.DesktopApp.Common.Abstractions.Services.Logging;
using PassMeta.DesktopApp.Common.Abstractions.Utils.PassMetaClient;
using PassMeta.DesktopApp.Common.Models.Dto.Response;
using PassMeta.DesktopApp.Common.Models.Settings;
using PassMeta.DesktopApp.Core.Models;
using PassMeta.DesktopApp.Core.Services.Extensions;

namespace PassMeta.DesktopApp.Core;

/// <summary>
/// Application context manager.
/// </summary>
public static class AppContext
{
    private static readonly BehaviorSubject<AppContextModel> CurrentSubject = new(new AppContextModel(new AppContextDto()));
    private static ILogService Logger => EnvironmentContainer.Resolve<ILogService>();

    /// <summary>
    /// Current application context.
    /// </summary>
    public static IAppContext Current => CurrentSubject.Value;

    /// <summary>
    /// Represents <see cref="Current"/>.
    /// </summary>
    public static IObservable<IAppContext> CurrentObservable => CurrentSubject;

    /// <summary>
    /// Load stored context and set it to <see cref="Current"/>.
    /// </summary>
    public static async Task LoadAndSetCurrentAsync()
    {
        AppContextDto? data = null;
            
        if (File.Exists(AppConfig.ContextFilePath))
        {
            try
            {
                data = JsonConvert.DeserializeObject<AppContextDto>(await File.ReadAllTextAsync(AppConfig.ContextFilePath));
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Context file reading failed");
            }
        }

        var context = new AppContextModel(data ?? new AppContextDto());
        SetCurrent(context);

        if (data is null)
        {
            _ = await SaveToFileAsync(context.ToDto());
        }
    }

    /// <summary>
    /// Refresh context from <paramref name="appConfig"/> and
    /// from the server (if <paramref name="passMetaClient"/> is online).
    /// </summary>
    public static async Task RefreshCurrentAsync(IAppConfig appConfig, IPassMetaClient passMetaClient)
    {
        if (appConfig.ServerUrl is null)
        {
            CurrentSubject.Value.ServerVersion = null;
            return;
        }

        if (passMetaClient.Online)
        {
            var response = await passMetaClient.Begin(PassMetaApi.General.GetInfo())
                .WithBadHandling()
                .ExecuteAsync<PassMetaInfo>();

            if (response?.Success is true)
            {
                var info = response.Data!;

                await ApplyAsync(appContext =>
                {
                    appContext.ServerId = info.AppId;
                    appContext.ServerVersion = info.AppVersion;
                    appContext.User = info.User;
                });
            }
        }
    }

    /// <summary>
    /// Edit current context model and flush changes.
    /// </summary>
    public static async Task ApplyAsync(Action<AppContextModel> setup)
    {
        var copy = CurrentSubject.Value.Copy();
        setup(copy);

        if (await SaveToFileAsync(copy.ToDto()))
        {
            SetCurrent(copy);
        }
    }

    private static async Task<bool> SaveToFileAsync(AppContextDto dto)
    {
        try
        {
            var path = AppConfig.ContextFilePath;
            var oldPath = AppConfig.ContextFilePath + ".old";
                
            var creatingNew = !File.Exists(path);
            if (creatingNew)
            {
                Logger.Info("Creating a new context file...");
            }
            else
            {
                File.SetAttributes(path,  File.GetAttributes(path) & ~FileAttributes.Hidden);
                if (File.Exists(oldPath))
                {
                    File.SetAttributes(path,  File.GetAttributes(oldPath) & ~FileAttributes.Hidden);
                }
                File.Move(path, oldPath, true);
                File.SetAttributes(oldPath, FileAttributes.Hidden);
            }
                
            await File.WriteAllTextAsync(path, JsonConvert.SerializeObject(dto));

            File.SetAttributes(path, FileAttributes.Hidden);
                
            if (creatingNew)
                Logger.Info("Context file created successfully");

            return true;
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Context file saving failed");
            return false;
        }
    }
    
    private static void SetCurrent(AppContextModel appContext) => CurrentSubject.OnNext(appContext);
}