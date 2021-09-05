using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
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
        private string _serverUrl;
        
        /// <summary>
        /// Server API.
        /// </summary>
        [JsonProperty("server"), NotNull]
        public string ServerUrl
        {
            get => _serverUrl;
            private set
            {
                _serverUrl = value;
                if (value is null) return;
                
                var path = value[(value.IndexOf("//", StringComparison.Ordinal) + 2)..];
                Domain = path[..path.LastIndexOf(':')];
            }
        }
        
        /// <summary>
        /// App cookies from server.
        /// </summary>
        [JsonProperty("cookies"), NotNull]
        public Dictionary<string, string> Cookies { get; private set; }
        
        /// <summary>
        /// App language.
        /// </summary>
        [JsonProperty("culture"), NotNull]
        public string CultureCode { get; private set; }

        /// <summary>
        /// App user.
        /// </summary>
        [JsonIgnore, AllowNull]
        public User User { get; private set; }

        /// <summary>
        /// A part of <see cref="ServerUrl"/>, which contains only domain.
        /// </summary>
        [JsonIgnore, AllowNull]
        public string Domain { get; private set; }
        
        /// <summary>
        /// Server version, indicates correct <see cref="ServerUrl"/> if not null.
        /// </summary>
        [JsonIgnore, AllowNull]
        public string ServerVersion { get; private set; }
        
        /// <summary>
        /// Translate package for server response messages.
        /// </summary>
        [JsonIgnore, NotNull]
        public Dictionary<string, Dictionary<string, string>> OkBadMessagesTranslatePack { get; private set; }

        /// <summary>
        /// Current app configuration.
        /// </summary>
        [NotNull]
        public static AppConfig Current { get; private set; } = new();

        /// <summary>
        /// Password files encryption salt.
        /// </summary>
        public static readonly byte[] PassFileSalt = Encoding.UTF8.GetBytes("PassMetaFileSalt");
        
        /// <summary>
        /// App supported languages (pairs: name-code).
        /// </summary>
        public static string[][] Cultures => new[]
        { 
            new[] { Resources.LANG__RU, "ru" },
            new[] { Resources.LANG__EN, "en" }
        };

        /// <summary>
        /// App version.
        /// </summary>
        public const string Version = "0.9.0";
        
        /// <summary>
        /// Path to app configuration file.
        /// </summary>
        private const string FilePath = ".config";

        private AppConfig()
        {
        }
        
        /// <summary>
        /// Set current app <see cref="User"/>.
        /// </summary>
        public void SetUser(User user)
        {
            Current.User = user;
        }

        /// <summary>
        /// Refresh current <see cref="Cookies"/> and save if changed.
        /// </summary>
        public void RefreshCookies(CookieCollection cookies)
        {
            lock (Current.Cookies)
            {
                var changed = false;
            
                for (var i = 0; i < cookies.Count; ++i)
                {
                    if (!Current.Cookies.TryGetValue(cookies[i].Name, out var cookie) || cookies[i].Value != cookie)
                    {
                        Current.Cookies[cookies[i].Name] = cookies[i].Value;
                        changed = true;
                    }
                }

                if (changed)
                {
                    _SaveToFileAsync(Current).GetAwaiter().GetResult();
                }
            }
        }
        
        /// <summary>
        /// Load and set app configuration to <see cref="Current"/>.
        /// </summary>
        /// <returns>Loaded or default configuration.</returns>
        public static async Task<AppConfig> LoadAndSetCurrentAsync()
        {
            AppConfig config = null;
            
            if (File.Exists(FilePath))
            {
                try
                {
                    config = JsonConvert.DeserializeObject<AppConfig>(await File.ReadAllTextAsync(FilePath));
                }
                catch (Exception ex)
                {
                    Locator.Current.GetService<IDialogService>()!
                        .ShowError(Resources.ERR__CONFIG_LOAD, more: ex.Message);
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
        public static async Task<Result<AppConfig>> CreateAndSetCurrentAsync(
            [AllowNull] string serverUrl, 
            [AllowNull] string culture)
        {
            var config = new AppConfig
            {
                ServerUrl = serverUrl ?? "",
                CultureCode = culture ?? ""
            };

            _CorrectConfig(config);

            if (!await _SaveToFileAsync(config))
                return new Result<AppConfig>(false);
            
            await _SetCurrentAsync(config);
            return new Result<AppConfig>(config);
        }

        private static async Task _SetCurrentAsync([NotNull] AppConfig config)
        {
            Current = config;
            Resources.Culture = new CultureInfo(config.CultureCode);
            
            if (config.ServerUrl.Length > 10)
            {
                var info = (await PassMetaApi.GetAsync<PassMetaInfo>("/info", true))?.Data;
                Current.ServerVersion = info?.AppVersion;
                Current.User = info?.User;
                Current.OkBadMessagesTranslatePack = info?.OkBadMessagesTranslatePack;
            }
            else
            {
                Current.ServerVersion = null;
                Current.User = null;
                Current.OkBadMessagesTranslatePack = null;
            }
            
            Current.OkBadMessagesTranslatePack ??= new Dictionary<string, Dictionary<string, string>>();
        }

        private static async Task<bool> _SaveToFileAsync(AppConfig config)
        {
            try
            {
                var attributes = File.GetAttributes(FilePath);
                attributes &= ~FileAttributes.Hidden;
                File.SetAttributes(FilePath, attributes);
                
                await File.WriteAllTextAsync(FilePath, JsonConvert.SerializeObject(config));

                attributes |= FileAttributes.Hidden;
                File.SetAttributes(FilePath, attributes);
                return true;
            }
            catch (Exception ex)
            {
                Locator.Current.GetService<IDialogService>()!
                    .ShowError(Resources.ERR__CONFIG_SAVE, more: ex.Message);
                return false;
            }
        }

        private static bool _CorrectConfig(AppConfig config)
        {
            var corrected = false;

            if (string.IsNullOrEmpty(config.CultureCode))
            {
                config.CultureCode = "ru";
                corrected = true;
            }

            if (config.ServerUrl is null || config.ServerUrl.Length < 11)
            {
                config.ServerUrl = "";
                corrected = true;
            }

            if (config.Cookies is null)
            {
                config.Cookies = new Dictionary<string, string>();
                corrected = true;
            }

            return corrected;
        }
    }
}