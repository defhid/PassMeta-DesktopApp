using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using PassMeta.DesktopApp.Common.Interfaces.Services;
using PassMeta.DesktopApp.Common.Models.Entities;
using PassMeta.DesktopApp.Common.Models.Entities.Response;
using Splat;

namespace PassMeta.DesktopApp.Core.Utils
{
    public class AppConfig
    {
        [JsonProperty("server")]
        public string ServerUrl { get; private set; }
        
        [JsonProperty("culture")]
        public string CultureString { get; private set; }
        
        [JsonIgnore]
        public User User { get; set; }
        
        [JsonIgnore]
        public string ServerVersion { get; private set; }

        [JsonIgnore]
        public bool IsServerUrlCorrect => ServerUrl.StartsWith("https://") && ServerUrl.Length > 10;
        
        public static AppConfig Current { get; private set; }

        public static readonly byte[] PassFileSalt = Encoding.UTF8.GetBytes("PassMetaFileSalt");

        public const string Version = "0.9.0";
        
        private const string FilePath = ".config";
        
        /// <summary>
        /// Load and set app configuration to <see cref="Current"/>.
        /// </summary>
        public static void Load()
        {
            Current = null;
            
            if (File.Exists(FilePath))
            {
                try
                {
                    Current = JsonConvert.DeserializeObject<AppConfig>(File.ReadAllText(FilePath));
                }
                catch (Exception ex)
                {
                    Locator.Current.GetService<IDialogService>()
                        .ShowErrorAsync(Resources.ERR__CONFIG_LOAD, more: ex.Message).GetAwaiter().GetResult();
                }
            }
            
            Current ??= new AppConfig();
            
            if (_CorrectConfig())
                _SaveToFileAsync().GetAwaiter().GetResult();

            Resources.Culture = new CultureInfo(Current.CultureString);
        }

        /// <summary>
        /// Set current <see cref="PassMetaInfo.User"/>, server <see cref="PassMetaInfo.AppVersion"/>
        /// to <see cref="Current"/>.
        /// </summary>
        public static void LoadPassMetaInfo([NotNull] PassMetaInfo info)
        {
            Current.ServerVersion = info.AppVersion;
            Current.User = info.User;
        }
        
        /// <summary>
        /// Save <see cref="Current"/> app configuration.
        /// </summary>
        /// <returns>Success?</returns>
        public static Task<bool> TrySaveAsync()
        {
            _CorrectConfig();
            return _SaveToFileAsync();
        }

        private static async Task<bool> _SaveToFileAsync()
        {
            try
            {
                var attributes = File.GetAttributes(FilePath);
                attributes &= FileAttributes.Hidden;
                File.SetAttributes(FilePath, attributes);
                
                await File.WriteAllTextAsync(FilePath, JsonConvert.SerializeObject(Current));

                attributes &= FileAttributes.Hidden;
                File.SetAttributes(FilePath, attributes);
                return true;
            }
            catch (Exception ex)
            {
                Locator.Current.GetService<IDialogService>()
                    .ShowErrorAsync(Resources.ERR__CONFIG_SAVE, more: ex.Message).GetAwaiter().GetResult();
                return false;
            }
        }

        private static bool _CorrectConfig()
        {
            var corrected = false;

            if (string.IsNullOrEmpty(Current.CultureString))
            {
                Current.CultureString = "ru";
                corrected = true;
            }

            if (Current.ServerUrl is null)
            {
                Current.ServerUrl = "";
                corrected = true;
            }

            return corrected;
        }
    }
}