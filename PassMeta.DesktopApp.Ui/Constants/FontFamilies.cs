namespace PassMeta.DesktopApp.Ui.Constants;

using Avalonia;
using Avalonia.Media;

/// <summary>
/// Avalonia fonts used in application.
/// </summary>
public static class FontFamilies
{
    public static readonly FontFamily Default = "$Default";
    public static readonly FontFamily SegoeMdl2 = (FontFamily) Application.Current!.Resources["SegoeMdl2"]!;
}