namespace PassMeta.DesktopApp.Core
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Reactive.Subjects;
    using System.Threading.Tasks;
    using Common.Abstractions;
    using Common.Abstractions.Utils;
    using Newtonsoft.Json;
    using PassMeta.DesktopApp.Common.Abstractions.Services;
    using PassMeta.DesktopApp.Common.Models.Dto.Response;
    using PassMeta.DesktopApp.Common.Models.Entities;
    using PassMeta.DesktopApp.Common.Models.Settings;
    using Utils;

    /// <inheritdoc />
    public class AppContext : IAppContext
    {
        private readonly AppContextData _originData;
        private List<Cookie> _cookies;

        /// <inheritdoc />
        public List<Cookie> Cookies
        {
            get => _cookies;
            set
            {
                _cookies = value;
                CookieContainer = CookiesHelper.BuildCookieContainer(value);
            }
        }

        /// <inheritdoc />
        public User? User { get; set; }

        /// <inheritdoc />
        public uint PassFilesCounter { get; set; }

        /// <inheritdoc />
        public string? ServerId { get; private set; }

        /// <inheritdoc />
        public string? ServerVersion { get; private set; }

        /// <inheritdoc />
        public CookieContainer CookieContainer { get; private set; }

        /// <summary>
        /// Current application context.
        /// </summary>
        public static IAppContext Current => CurrentSubject.Value;

        /// <summary>
        /// Represents <see cref="Current"/>.
        /// </summary>
        public static IObservable<IAppContext> CurrentObservable => CurrentSubject;

        private static readonly BehaviorSubject<IAppContext> CurrentSubject = new(new AppContext(new AppContextData()));
        
        #region Services
        
        private static ILogService Logger => EnvironmentContainer.Resolve<ILogService>();

        #endregion

        private AppContext(AppContextData data)
        {
            _originData = data;
            _cookies = data.Cookies ?? new List<Cookie>();
            CookieContainer = CookiesHelper.BuildCookieContainer(_cookies);
            User = data.User;
            PassFilesCounter = data.PassFilesCounter ?? 0;
            ServerId = data.ServerId;
        }

        /// <summary>
        /// Load and set context to <see cref="Current"/>.
        /// </summary>
        public static async Task LoadAndSetCurrentAsync()
        {
            AppContextData? data = null;
            
            if (File.Exists(AppConfig.ContextFilePath))
            {
                try
                {
                    data = JsonConvert.DeserializeObject<AppContextData>(await File.ReadAllTextAsync(AppConfig.ContextFilePath));
                }
                catch (Exception ex)
                {
                    Logger.Error(ex, "Context file reading failed");
                }
            }

            if (data is null)
            {
                var current = (AppContext)Current;

                current.User = null;
                current.Cookies = new List<Cookie>();
                current.PassFilesCounter = 0;
                current.ServerId = null;

                await FlushCurrentAsync();
            }
            else
            {
                data.Cookies ??= new List<Cookie>();
                data.PassFilesCounter ??= 0;

                CurrentSubject.OnNext(new AppContext(data));
            }
        }

        /// <summary>
        /// Refresh context from the server.
        /// </summary>
        public static async Task RefreshCurrentFromServerAsync(IPassMetaClient passMetaClient, bool checkConnection = true)
        {
            var current = (AppContext)Current;
            
            if (AppConfig.Current.ServerUrl is null)
            {
                current.ServerVersion = null;
            }
            else
            {
                if (!checkConnection || await passMetaClient.CheckConnectionAsync(true, true))
                {
                    var response = await passMetaClient.Get("info").WithBadHandling().ExecuteAsync<PassMetaInfo>();
                    if (response?.Success is true)
                    {
                        var info = response.Data!;

                        current.ServerId = info.AppId;
                        current.ServerVersion = info.AppVersion;
                        current.User = info.User;

                        await FlushCurrentAsync();
                    }
                }
            }
        }

        /// <summary>
        /// Save context changes and reload current model.
        /// </summary>
        public static async Task FlushCurrentAsync()
        {
            var data = new AppContextData
            {
                User = Current.User,
                Cookies = Current.Cookies.ToList(),
                PassFilesCounter = Current.PassFilesCounter,
                ServerId = Current.ServerId
            };

            var old = (AppContext)Current;

            if (data.User?.Id != old._originData.User?.Id && data.User is null)
            {
                data.Cookies.Clear();
            }

            await _SaveToFileAsync(data);

            var current = new AppContext(data)
            {
                ServerVersion = old.ServerVersion
            };

            CurrentSubject.OnNext(current);
        }

        private static async Task _SaveToFileAsync(AppContextData data)
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
                
                await File.WriteAllTextAsync(path, JsonConvert.SerializeObject(data));

                File.SetAttributes(path, FileAttributes.Hidden);
                
                if (creatingNew)
                    Logger.Info("Context file created successfully");
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Context file saving failed");
            }
        }
    }
}