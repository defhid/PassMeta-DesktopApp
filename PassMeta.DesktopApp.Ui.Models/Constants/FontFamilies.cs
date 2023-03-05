using Avalonia;
using Avalonia.Media;

namespace PassMeta.DesktopApp.Ui.Models.Constants;

/// <summary>
/// Avalonia fonts used in application.
/// </summary>
public static class FontFamilies
{
    public static readonly FontFamily Default = "$Default";
    public static readonly FontFamily SegoeMdl2 = (FontFamily) Application.Current!.Resources["SegoeMdl2"]!;
}