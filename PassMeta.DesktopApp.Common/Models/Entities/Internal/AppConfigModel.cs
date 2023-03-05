using PassMeta.DesktopApp.Common.Abstractions.App;
using PassMeta.DesktopApp.Common.Constants;
using PassMeta.DesktopApp.Common.Models.Dto.Internal;

namespace PassMeta.DesktopApp.Common.Models.Entities.Internal;

/// <inheritdoc />
public class AppConfigModel : IAppConfig
{
    private const int MinUrlLength = 11;
    
    /// <inheritdoc />
    public AppCulture Culture { get; set; }

    /// <inheritdoc />
    public string? ServerUrl { get; set; }

    /// <inheritdoc />
    public bool HidePasswords { get; set; }

    /// <inheritdoc />
    public bool DevMode { get; set; }

    /// <inheritdoc />
    public bool DebugMode { get; set; }

    /// <inheritdoc />
    public int DefaultPasswordLength { get; set; }

    /// <summary></summary>
    public AppConfigModel(AppConfigDto dto)
    {
        AppCulture.TryParse(dto.CultureCode ?? string.Empty, out var culture);
        Culture = culture;

        var serverUrl = dto.ServerUrl?.Trim();
        ServerUrl = string.IsNullOrEmpty(serverUrl) || serverUrl.Length < MinUrlLength ? null : serverUrl;
        
        HidePasswords = dto.HidePasswords ?? false;
        DevMode = dto.DevMode ?? false;
        DebugMode = dto.DebugMode ?? false;
        DefaultPasswordLength = dto.DefaultPasswordLength ?? 12;
    }

    /// <summary></summary>
    public AppConfigDto ToDto() => new()
    {
        CultureCode = Culture.Code,
        ServerUrl = ServerUrl,
        HidePasswords = HidePasswords,
        DevMode = DevMode,
        DebugMode = DebugMode,
        DefaultPasswordLength = DefaultPasswordLength,
    };

    /// <summary>
    /// Get a new model with copied properties.
    /// </summary>
    public AppConfigModel Copy() => (AppConfigModel) MemberwiseClone();
}