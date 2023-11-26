using System.Threading.Tasks;
using PassMeta.DesktopApp.Common.Abstractions.App;
using PassMeta.DesktopApp.Common.Models.Presets;

namespace PassMeta.DesktopApp.Core.Utils;

/// <inheritdoc />
public class AppPresetsManager : IAppPresetsManager
{
    /// <inheritdoc />
    public Task LoadAsync()
    {
        PasswordGeneratorPresets = new PasswordGeneratorPresets
        {
            Length = 12,
            IncludeDigits = true,
            IncludeLowercase = true,
            IncludeUppercase = true,
            IncludeSpecial = true,
        };

        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public PasswordGeneratorPresets PasswordGeneratorPresets { get; private set; } = new();
}