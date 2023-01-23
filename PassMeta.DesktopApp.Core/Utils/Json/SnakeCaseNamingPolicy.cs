using System.Text;
using System.Text.Json;

namespace PassMeta.DesktopApp.Core.Utils.Json;

/// <summary>
/// Snake case naming policy.
/// </summary>
public class SnakeCaseNamingPolicy : JsonNamingPolicy
{
    /// <summary>
    /// Singleton.
    /// </summary>
    public static readonly SnakeCaseNamingPolicy Instance = new();

    /// <inheritdoc />
    public override string ConvertName(string name)
    {
        var builder = new StringBuilder(name.Length);
        var isUpperFound = false;

        for (var i = 0; i < name.Length; ++i)
        {
            var ch = name[i];
            
            if (i > 0 && char.IsUpper(ch))
            {
                builder.Append('_');
                builder.Append(char.ToLower(ch));
                isUpperFound = true;
            }
            else
            {
                builder.Append(ch);
            }
        }

        return isUpperFound ? builder.ToString() : name;
    }
}