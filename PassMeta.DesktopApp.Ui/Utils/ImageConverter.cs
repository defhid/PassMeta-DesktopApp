using System;
using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Media.Imaging;
using Avalonia.Platform;

namespace PassMeta.DesktopApp.Ui.Utils;

public class ImageConverter : IValueConverter
{
    private static readonly string AssemblyName = typeof(ImageConverter).Assembly.GetName().Name!;

    public static Bitmap GetAvaloniaResourceBitMap(string relativePath)
    {
        var uri = new Uri($"avares://{AssemblyName}/{relativePath}");

        return new Bitmap(AssetLoader.Open(uri));
    }

    public static Uri GetManifestResourcePath(string relativePath) => new($"resm:{AssemblyName}.{relativePath}");

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value is null ? null : GetAvaloniaResourceBitMap((string)value);
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException(
            nameof(ImageConverter) + '.' + nameof(ConvertBack) + " method not implemented!");
    }
}