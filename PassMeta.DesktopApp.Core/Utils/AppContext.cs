namespace PassMeta.DesktopApp.Core.Utils
{
    using Common.Interfaces.Services;
    using Common.Interfaces.Mapping;
    using Common.Models.Dto.Response;
    using Common.Models.Entities;
    
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Threading.Tasks;
    using Common.Utils.Mapping;
    using Mapping;
    using Newtonsoft.Json;

    /// <summary>
    /// Application context: cookies, user, etc.
    /// </summary>
    public class AppContext
    {
        private CookieContainer _cookieContainer = new();

        /// <summary>
        /// Cookies from the server.
        /// </summary>
        [JsonProperty("cookies")]
        public List<Cookie>? Cookies { get; private set; }

        /// <summary>
        /// Application user.
        /// </summary>
        [JsonProperty("user")]
        public User? User { get; private set; }
        
        /// <summary>
        /// Total count of locally created passfiles.
        /// </summary>
        [JsonProperty("pf")]
        public uint PassFilesCounter { get; set; }

        /// <summary>
        /// Application user id. May be 0, if <see cref="User"/> is null.
        /// </summary>
        [JsonIgnore]
        public int UserId => User?.Id ?? 0;
        
        /// <summary>
        /// <see cref="Cookies"/> in form of <see cref="CookieContainer"/>.
        /// </summary>
        [JsonIgnore]
        public CookieContainer CookieContainer => _cookieContainer;

        /// <summary>
        /// Server version. If not null, indicates correct <see cref="AppConfig.ServerUrl"/>
        /// and internet connection has been established at least once.
        /// </summary>
        [JsonIgnore]
        public string? ServerVersion { get; private set; }

        /// <summary>
        /// Translate package for server response messages.
        /// </summary>
        [JsonIgnore]
        public IMapper<string, string> OkBadMessagesMapper { get; private set; } 
            = new SimpleMapper<string, string>(Enumerable.Empty<IMapping<string, string>>());
        
        /// <summary>
        /// Current application context.
        /// </summary>
        public static AppContext Current { get; private set; } = new();

        #region Services
        
        private static ILogService Logger => EnvironmentContainer.Resolve<ILogService>();

        #endregion

        private AppContext()
        {
        }

        /// <summary>
        /// Save <see cref="Current"/> context.
        /// </summary>
        public static Task SaveCurrentAsync() => _SaveToFileAsync(Current);

        /// <summary>
        /// Load and set context to <see cref="Current"/>.
        /// </summary>
        /// <returns>Loaded or default context.</returns>
        public static async Task LoadAndSetCurrentAsync()
        {
            AppContext? context = null;
            
            if (File.Exists(AppConfig.ContextFilePath))
            {
                try
                {
                    context = JsonConvert.DeserializeObject<AppContext>(await File.ReadAllTextAsync(AppConfig.ContextFilePath));
                    if (context is not null)
                        _RefreshCookieContainer(context);
                }
                catch (Exception ex)
                {
                    Logger.Error(ex, "Context file reading failed");
                }
            }

            if (context is null)
            {
                context = new AppContext();
                await _SaveToFileAsync(context);
            }

            Current = context;
        }
        
        /// <summary>
        /// Refresh current context from the server.
        /// </summary>
        public static async Task RefreshFromServerAsync(bool checkConnection = true)
        {
            if (AppConfig.Current.ServerUrl is null)
            {
                Current.ServerVersion = null;
            }
            else
            {
                if (!checkConnection || await PassMetaApi.CheckConnectionAsync(true))
                {
                    var response = await PassMetaApi.GetAsync<PassMetaInfo>("info", true);
                    if (response?.Success is true)
                    {
                        var info = response.Data!;
                        Current.ServerVersion = info.AppVersion;
                        Current.User = info.User;
                        
                        var pack = info.OkBadMessagesTranslatePack ??
                                   new Dictionary<string, Dictionary<string, string>>(0);
                        Current.OkBadMessagesMapper = new SimpleMapper<string, string>(
                            pack.Select(pair => new MapToTranslate<string>(pair.Key, pair.Value)));
                        
                        await _SaveToFileAsync(Current);
                    }
                }
            }
        }
        
        /// <summary>
        /// Set current <see cref="User"/>.
        /// </summary>
        public static async Task SetUserAsync(User? user)
        {
            Current.User = user;

            if (user is null && Current.Cookies?.Any() is true)
            {
                Current.Cookies.Clear();
                _RefreshCookieContainer(Current);
            }
            
            await _SaveToFileAsync(Current);
        }

        /// <summary>
        /// Refresh current <see cref="Cookies"/> and save if changed.
        /// </summary>
        public static void RefreshCookies(CookieCollection freshCookies)
        {
            Current.Cookies ??= new List<Cookie>();
            var changed = false;
            
            lock (Current.Cookies)
            {
                foreach (Cookie fresh in freshCookies)
                {
                    var currentIndex = Current.Cookies.FindIndex(c => c.Equals(fresh));
                    if (currentIndex < 0)
                    {
                        Current.Cookies.Add(fresh);
                        changed = true;
                    }
                    else if (Current.Cookies[currentIndex].Expires != fresh.Expires)
                    {
                        Current.Cookies[currentIndex] = fresh;
                        changed = true;
                    }
                }

                if (changed)
                {
                    if (Current.Cookies.Count > freshCookies.Count)
                    {
                        for (var i = Current.Cookies.Count - 1; i >= 0; --i)
                        {
                            if (!freshCookies.Any(c => c.Equals(Current.Cookies[i])))
                            {
                                Current.Cookies.RemoveAt(i);
                            }
                        }
                    }
                    
                    _RefreshCookieContainer(Current);
                }
            }

            if (changed)
            {
                Task.Run(() => _SaveToFileAsync(Current)).GetAwaiter().GetResult();
            }
        }

        private static void _RefreshCookieContainer(AppContext context)
        {
            context._cookieContainer = new CookieContainer();
            if (context.Cookies is null) return;
            
            foreach (var cookie in context.Cookies)
            {
                // new cookie to attach to requests correctly
                context._cookieContainer.Add(new Cookie(cookie.Name, cookie.Value, null, cookie.Domain));
            }
        }

        private static async Task _SaveToFileAsync(AppContext context)
        {
            try
            {
                FileAttributes attributes = default;
                
                var creatingNew = !File.Exists(AppConfig.ContextFilePath);
                if (creatingNew)
                {
                    Logger.Info("Creating a new context file...");
                }
                else
                {
                    attributes = File.GetAttributes(AppConfig.ContextFilePath);
                    attributes &= ~FileAttributes.Hidden;
                    File.SetAttributes(AppConfig.ContextFilePath, attributes);
                }
                
                await File.WriteAllTextAsync(AppConfig.ContextFilePath, JsonConvert.SerializeObject(context));

                attributes |= FileAttributes.Hidden;
                File.SetAttributes(AppConfig.ContextFilePath, attributes);
                
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