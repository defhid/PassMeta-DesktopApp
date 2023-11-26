using System.Collections.Generic;
using System.Linq;
using Avalonia.Media;
using PassMeta.DesktopApp.Common;
using PassMeta.DesktopApp.Common.Abstractions.Utils.ValueMapping;
using PassMeta.DesktopApp.Common.Utils.ValueMapping;

namespace PassMeta.DesktopApp.Ui.Models.Constants;

public class PassFileColor
{
    /// <summary>
    /// Color in hex format without #, upper case.
    /// </summary>
    public readonly string? Hex;
        
    /// <summary>
    /// Color parsed in brush.
    /// </summary>
    public ISolidColorBrush? Brush { get; }
        
    /// <summary>
    /// PassMeta common color name.
    /// </summary>
    public string Name => ColorToName.Map(this, "?");

    private PassFileColor(string? hex)
    {
        Hex = hex?.TrimStart('#').ToUpper();
        Brush = hex is null ? null : new SolidColorBrush(Color.Parse(hex));
    }

    public static readonly PassFileColor None = new(null);
    public static readonly PassFileColor Red = new("#F11D1D");
    public static readonly PassFileColor Blue = new("#1E90FF");
    public static readonly PassFileColor Gray = new("#808080");
    public static readonly PassFileColor Pink = new("#FF69B4");
    public static readonly PassFileColor Teal = new("#02B5AB");
    public static readonly PassFileColor Green = new("#32CD32");
    public static readonly PassFileColor Orange = new("#FFA500");
    public static readonly PassFileColor Purple = new("#9370DB");
    public static readonly PassFileColor Yellow = new("#D5D500");

    private static readonly ValuesMapper<PassFileColor, string> ColorToName = new IValueMapping<PassFileColor, string>[]
    {
        new MapToString<PassFileColor>(None, "-"),
        new MapToResource<PassFileColor>(Red, () => Resources.PASSFILE_COLOR__RED),
        new MapToResource<PassFileColor>(Blue, () => Resources.PASSFILE_COLOR__BLUE),
        new MapToResource<PassFileColor>(Gray, () => Resources.PASSFILE_COLOR__GREY),
        new MapToResource<PassFileColor>(Pink, () => Resources.PASSFILE_COLOR__PINK),
        new MapToResource<PassFileColor>(Teal, () => Resources.PASSFILE_COLOR__TEAL),
        new MapToResource<PassFileColor>(Green, () => Resources.PASSFILE_COLOR__GREEN),
        new MapToResource<PassFileColor>(Orange, () => Resources.PASSFILE_COLOR__ORANGE),
        new MapToResource<PassFileColor>(Purple, () => Resources.PASSFILE_COLOR__PURPLE),
        new MapToResource<PassFileColor>(Yellow, () => Resources.PASSFILE_COLOR__YELLOW),
    };

    public static IReadOnlyList<PassFileColor> List { get; } =
        ColorToName.GetMappings().Select(map => map.From).ToArray();
}