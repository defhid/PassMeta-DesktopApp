namespace PassMeta.DesktopApp.Core.Utils
{
    using DesktopApp.Common;
    using DesktopApp.Common.Interfaces.Services;
    using DesktopApp.Common.Models;
    using System;
    using System.Globalization;
    using System.IO;
    using System.Reflection;
    using System.Threading.Tasks;
    using Common.Constants;
    using Common.Interfaces;
    using Newtonsoft.Json;

    /// <summary>
    /// Application configuration.
    /// </summary>
    public class AppConfig
    {
        private string? _serverUrl;
        
        private AppCulture _culture = AppCulture.Default;
        
        /// <summary>
        /// PassMeta server API. Non-empty string or null.
        /// </summary>
        [JsonProperty("server")]
        public string? ServerUrl
        {
            get => _serverUrl;
            private set => _serverUrl = string.IsNullOrWhiteSpace(value) || value.Length < MinUrlLength ? null : value;
        }
        
        /// <summary>
        /// Application language code.
        /// </summary>
        [JsonProperty("culture")]
        public string CultureCode 
        {
            get => _culture.Code;
            private set => AppCulture.TryParse(value, out _culture);
        }

        /// <summary>
        /// Hide user passwords.
        /// </summary>
        [JsonProperty("hide_pwd")]
        public bool HidePasswords { get; private set; }

        /// <summary>
        /// Default length for password generator.
        /// </summary>
        [JsonProperty("default_password_length")]
        public int DefaultPasswordLength => 12;

        /// <summary>
        /// Current application config.
        /// </summary>
        public static AppConfig Current { get; private set; } = new();

        #region Consts

        private const int MinUrlLength = 11;

        /// <summary>
        /// Application version.
        /// </summary>
        public static readonly string Version = Assembly.GetExecutingAssembly().GetName().Version?.ToString()[..^2] ?? "?";

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

        #endregion

        #region Events

        /// <summary>
        /// Invokes when current <see cref="CultureCode"/> changes.
        /// </summary>
        public static event Action? OnCultureChanged;

        #endregion

        #region Services
        
        private static IDialogService DialogService => EnvironmentContainer.Resolve<IDialogService>();
        private static ILogService Logger => EnvironmentContainer.Resolve<ILogService>();

        #endregion

        private AppConfig()
        {
        }

        /// <summary>
        /// Load and set app configuration to <see cref="Current"/>.
        /// </summary>
        /// <returns>Loaded or default configuration.</returns>
        public static async Task LoadAndSetCurrentAsync()
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
                    Logger.Error(ex, "Configuration file reading failed");
                    DialogService.ShowError(Resources.APP_CONFIG__LOAD_ERR);
                }
            }

            if (config is null)
            {
                config = new AppConfig();
                var result = await _SaveToFileAsync(config);
                if (result.Bad)
                    DialogService.ShowError(result.Message!);
            }

            await _SetCurrentAsync(config);
        }

        /// <summary>
        /// Create and set app configuration to <see cref="Current"/>.
        /// </summary>
        /// <returns>Success + created configuration.</returns>
        public static async Task<IDetailedResult> CreateAndSetCurrentAsync(
            string? serverUrl,
            AppCulture? culture,
            bool hidePasswords)
        {
            var config = new AppConfig
            {
                ServerUrl = serverUrl,
                _culture = culture ?? AppCulture.Default,
                HidePasswords = hidePasswords
            };

            var result = await _SaveToFileAsync(config);
            if (result.Bad) return result;

            await _SetCurrentAsync(config);
            return Result.Success();
        }

        private static async Task _SetCurrentAsync(AppConfig config)
        {
            var cultureChanged = Current.CultureCode != config.CultureCode;
            var serverChanged = Current.ServerUrl != config.ServerUrl;

            Current = config;
            Resources.Culture = new CultureInfo(config.CultureCode);

            if (serverChanged)
            {
                try
                {
                    await AppContext.RefreshFromServerAsync();
                    await PassFileManager.ReloadAsync(false);
                }
                catch (Exception ex)
                {
                    Logger.Error(ex, "Processing of server-changing event failed");
                }
            }

            if (cultureChanged)
            {
                try
                {
                    OnCultureChanged?.Invoke();
                }
                catch (Exception ex)
                {
                    Logger.Error(ex, "Processing of culture-changing event failed");
                }
            }
        }

        private static async Task<IDetailedResult> _SaveToFileAsync(AppConfig config)
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

                await File.WriteAllTextAsync(ConfigFilePath, JsonConvert.SerializeObject(config));

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
    }
}