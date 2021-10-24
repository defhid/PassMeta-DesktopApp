using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using PassMeta.DesktopApp.Common;
using PassMeta.DesktopApp.Common.Interfaces.Services;
using PassMeta.DesktopApp.Common.Models;
using PassMeta.DesktopApp.Common.Models.Entities;
using PassMeta.DesktopApp.Common.Models.Entities.Response;
using Splat;

namespace PassMeta.DesktopApp.Core.Utils
{
    /// <summary>
    /// Application configuration.
    /// </summary>
    public class AppConfig
    {
        private string? _serverUrl;
        
        /// <summary>
        /// Server API.
        /// </summary>
        [JsonProperty("server")]
        public string? ServerUrl
        {
            get => _serverUrl;
            set
            {
                _serverUrl = value;
                if (value is null) return;
                
                var path = value[(value.IndexOf("//", StringComparison.Ordinal) + 2)..];
                Domain = path[..path.LastIndexOf(':')];
            }
        }

        private Dictionary<string, string> _cookies = new();

        /// <summary>
        /// App cookies from server.
        /// </summary>
        [JsonProperty("cookies")]
        public Dictionary<string, string> Cookies
        {
            get => _cookies;
            // ReSharper disable once ConstantNullCoalescingCondition
            set => _cookies = value ?? new Dictionary<string, string>();
        }

        
        private string _cultureCode = "";
        
        /// <summary>
        /// App language.
        /// </summary>
        [JsonProperty("culture")]
        public string CultureCode {
            get => _cultureCode;
            // ReSharper disable once ConstantNullCoalescingCondition
            set => _cultureCode = value ?? string.Empty;
        }

        /// <summary>
        /// App user.
        /// </summary>
        [JsonProperty("user")]
        public User? User { get; set; }
        
        /// <summary>
        /// Passfiles encryption keyphrase.
        /// </summary>
        [JsonIgnore]
        public string? PassFilesKeyPhrase { get; set; }

        /// <summary>
        /// A part of <see cref="ServerUrl"/>, which contains only domain.
        /// </summary>
        [JsonIgnore]
        public string? Domain { get; private set; }
        
        /// <summary>
        /// Server version, indicates correct <see cref="ServerUrl"/>
        /// and internet connection if not null.
        /// </summary>
        [JsonIgnore]
        public string? ServerVersion { get; private set; }

        /// <summary>
        /// Translate package for server response messages.
        /// </summary>
        [JsonIgnore]
        public Dictionary<string, Dictionary<string, string>> OkBadMessagesTranslatePack { get; private set; } = new();

        /// <summary>
        /// Current app configuration.
        /// </summary>
        public static AppConfig Current { get; private set; } = new();

        /// <summary>
        /// Password files encryption salt.
        /// </summary>
        public static readonly byte[] PassFileSalt = Encoding.UTF8.GetBytes("PassMetaFileSalt");
        
        /// <summary>
        /// App supported languages (pairs: name-code).
        /// </summary>
        public static string[][] AppCultures => new[]
        { 
            new[] { Resources.LANG__RU, "ru" },
            new[] { Resources.LANG__EN, "en" }
        };

        /// <summary>
        /// App version.
        /// </summary>
        public const string Version = "0.9.0";
        
        /// <summary>
        /// Path to passfiles storage.
        /// </summary>
        public const string PassFilesPath = "/passfiles";
        
        /// <summary>
        /// Path to app configuration file.
        /// </summary>
        private const string ConfigFilePath = ".config";

        private AppConfig()
        {
        }
        
        /// <summary>
        /// Set current app <see cref="User"/>.
        /// </summary>
        public Task SetUserAsync(User? user)
        {
            Current.User = user;

            if (user is null && Cookies.Any())
            {
                Cookies.Clear();
                return _SaveToFileAsync(Current);
            }
            
            return Task.CompletedTask;
        }

        /// <summary>
        /// Refresh current <see cref="Cookies"/> and save if changed.
        /// </summary>
        public void RefreshCookies(CookieCollection cookies)
        {
            lock (Cookies)
            {
                var changed = false;
            
                for (var i = 0; i < cookies.Count; ++i)
                {
                    if (!Cookies.TryGetValue(cookies[i].Name, out var cookie) || cookies[i].Value != cookie)
                    {
                        Cookies[cookies[i].Name] = cookies[i].Value;
                        changed = true;
                    }
                }

                if (changed)
                {
                    _SaveToFileAsync(this).GetAwaiter().GetResult();
                }
            }
        }
        
        /// <summary>
        /// Refresh information from server.
        /// </summary>
        public async Task RefreshFromServerAsync()
        {
            if (ServerUrl?.Length > 10)
            {
                var infoResponse = await PassMetaApi.GetAsync<PassMetaInfo>("/info", true);
                if (infoResponse is null)
                {
                    ServerVersion = null;
                    User = null;
                    OkBadMessagesTranslatePack = new Dictionary<string, Dictionary<string, string>>();
                }
                else if (infoResponse.Success)
                {
                    ServerVersion = infoResponse.Data!.AppVersion;
                    User = infoResponse.Data!.User;
                    OkBadMessagesTranslatePack = infoResponse.Data!.OkBadMessagesTranslatePack;
                }
                else
                {
                    ServerVersion = "?";
                    User = null;
                    OkBadMessagesTranslatePack = new Dictionary<string, Dictionary<string, string>>();
                }
            }
            else
            {
                ServerVersion = null;
                User = null;
                OkBadMessagesTranslatePack = new Dictionary<string, Dictionary<string, string>>();
            }
        }
        
        /// <summary>
        /// Load and set app configuration to <see cref="Current"/>.
        /// </summary>
        /// <returns>Loaded or default configuration.</returns>
        public static async Task<AppConfig> LoadAndSetCurrentAsync()
        {
            AppConfig? config = null;
            
            if (File.Exists(ConfigFilePath))
            {
                try
                {
                    config = JsonConvert.DeserializeObject<AppConfig>(await File.ReadAllTextAsync(ConfigFilePath));
                }
                catch (Exception ex)
                {
                    await Locator.Current.GetService<IDialogService>()!
                        .ShowErrorAsync(Resources.ERR__CONFIG_LOAD, more: ex.Message);
                }
            }
            
            config ??= new AppConfig();

            if (_CorrectConfig(config))
                await _SaveToFileAsync(config);

            await _SetCurrentAsync(config);
            return config;
        }

        /// <summary>
        /// Create and set app configuration to <see cref="Current"/>.
        /// </summary>
        /// <returns>Success + created configuration.</returns>
        public static async Task<Result<AppConfig>> CreateAndSetCurrentAsync(string? serverUrl, string? culture)
        {
            var config = new AppConfig
            {
                ServerUrl = serverUrl ?? "",
                CultureCode = culture ?? ""
            };

            _CorrectConfig(config);

            if (config.ServerUrl == Current.ServerUrl)
            {
                config.Cookies = Current.Cookies;
            }

            if (!await _SaveToFileAsync(config))
                return new Result<AppConfig>(false);
            
            await _SetCurrentAsync(config);
            return new Result<AppConfig>(config);
        }

        private static Task _SetCurrentAsync(AppConfig config)
        {
            Current = config;
            Resources.Culture = new CultureInfo(config.CultureCode);

            return Current.RefreshFromServerAsync();
        }

        private static async Task<bool> _SaveToFileAsync(AppConfig config)
        {
            try
            {
                var attributes = File.GetAttributes(ConfigFilePath);
                attributes &= ~FileAttributes.Hidden;
                File.SetAttributes(ConfigFilePath, attributes);
                
                await File.WriteAllTextAsync(ConfigFilePath, JsonConvert.SerializeObject(config));

                attributes |= FileAttributes.Hidden;
                File.SetAttributes(ConfigFilePath, attributes);
                return true;
            }
            catch (Exception ex)
            {
                await Locator.Current.GetService<IDialogService>()!
                    .ShowErrorAsync(Resources.ERR__CONFIG_SAVE, more: ex.Message);
                return false;
            }
        }

        private static bool _CorrectConfig(AppConfig config)
        {
            var corrected = false;

            if (AppCultures.All(c => c[1] != config.CultureCode))
            {
                config.CultureCode = "ru";
                corrected = true;
            }

            if (config.ServerUrl is null || config.ServerUrl.Length < 11)
            {
                config.ServerUrl = "";
                corrected = true;
            }

            return corrected;
        }
    }
}