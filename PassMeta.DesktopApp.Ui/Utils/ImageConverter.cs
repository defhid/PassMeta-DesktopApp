namespace PassMeta.DesktopApp.Ui.Utils
{
    using System;
    using System.Globalization;
    using System.Reflection;
    using Avalonia;
    using Avalonia.Data.Converters;
    using Avalonia.Media.Imaging;
    using Avalonia.Platform;
    
    public class ImageConverter : IValueConverter
    {
        private static readonly string AssemblyName = Assembly.GetEntryAssembly()!.GetName().Name!;
        
        public static Bitmap GetAvaloniaResourceBitMap(string relativePath)
        {
            var uri = new Uri($"avares://{AssemblyName}/{relativePath}");
            
            var assets = AvaloniaLocator.Current.GetService<IAssetLoader>()!;
            return new Bitmap(assets.Open(uri));
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
}