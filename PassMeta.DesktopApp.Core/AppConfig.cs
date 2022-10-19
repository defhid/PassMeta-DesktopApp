namespace PassMeta.DesktopApp.Core
{
    using System;
    using System.IO;
    using System.Reactive.Subjects;
    using System.Reflection;
    using System.Threading.Tasks;
    using Newtonsoft.Json;
    using PassMeta.DesktopApp.Common;
    using PassMeta.DesktopApp.Common.Abstractions;
    using PassMeta.DesktopApp.Common.Abstractions.Services;
    using PassMeta.DesktopApp.Common.Constants;
    using PassMeta.DesktopApp.Common.Models;
    using PassMeta.DesktopApp.Common.Models.Settings;

    /// <summary>
    /// Application configuration.
    /// </summary>
    public class AppConfig : IAppConfig
    {
        private readonly AppConfigData _data;

        /// <inheritdoc />
        public string CultureCode => _data.CultureCode!;

        /// <inheritdoc />
        public string? ServerUrl => _data.ServerUrl;

        /// <inheritdoc />
        public bool HidePasswords => _data.HidePasswords!.Value;

        /// <inheritdoc />
        public bool DevMode => _data.DevMode!.Value;

        /// <inheritdoc />
        public int DefaultPasswordLength => _data.DefaultPasswordLength!.Value;

        /// <summary>
        /// Current application config.
        /// </summary>
        public static IAppConfig Current => CurrentSubject.Value;

        /// <summary>
        /// Represents <see cref="Current"/>.
        /// </summary>
        public static IObservable<IAppConfig> CurrentObservable => CurrentSubject;

        private static readonly BehaviorSubject<IAppConfig> CurrentSubject = new(new AppConfig(new AppConfigData()));

        #region Consts

        private const int MinUrlLength = 11;

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

        #region Services
        
        private static IDialogService DialogService => EnvironmentContainer.Resolve<IDialogService>();
        private static ILogService Logger => EnvironmentContainer.Resolve<ILogService>();

        #endregion

        private AppConfig(AppConfigData data)
        {
            _data = _Prepare(data);
        }

        /// <summary>
        /// Load and set app configuration to <see cref="Current"/>.
        /// </summary>
        public static async Task LoadAndSetCurrentAsync()
        {
            AppConfigData? data = null;
            
            if (File.Exists(ConfigFilePath))
            {
                try
                {
                    data = JsonConvert.DeserializeObject<AppConfigData>(await File.ReadAllTextAsync(ConfigFilePath));
                }
                catch (Exception ex)
                {
                    Logger.Error(ex, "Configuration file reading failed");
                    DialogService.ShowError(Resources.APP_CONFIG__LOAD_ERR);
                }
            }

            if (data is null)
            {
                data = _Prepare(new AppConfigData());
                var result = await _SaveToFileAsync(data);
                if (result.Bad)
                    DialogService.ShowError(result.Message!);
            }

            CurrentSubject.OnNext(new AppConfig(data));
        }

        /// <summary>
        /// Create and set app configuration to <see cref="Current"/>.
        /// </summary>
        public static async Task<IDetailedResult> CreateAndSetCurrentAsync(AppConfigData data)
        {
            _Prepare(data);

            var result = await _SaveToFileAsync(data);
            if (result.Bad) return result;

            CurrentSubject.OnNext(new AppConfig(data));

            return Result.Success();
        }

        private static AppConfigData _Prepare(AppConfigData data)
        {
            AppCulture.TryParse(data.CultureCode ?? string.Empty, out var culture);
            data.CultureCode = culture.Code;

            data.ServerUrl = data.ServerUrl?.Trim();
            data.ServerUrl = string.IsNullOrEmpty(data.ServerUrl) || data.ServerUrl.Length < MinUrlLength ? null : data.ServerUrl;

            data.HidePasswords ??= false;
            data.DevMode ??= false;
            data.DefaultPasswordLength ??= 12;

            return data;
        }

        private static async Task<IDetailedResult> _SaveToFileAsync(AppConfigData data)
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

                await File.WriteAllTextAsync(ConfigFilePath, JsonConvert.SerializeObject(data));

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