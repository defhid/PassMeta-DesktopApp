using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Reactive;
using Avalonia.Threading;

namespace AvaloniaGif;

public class GifImage : Control
{
    public static readonly StyledProperty<Uri> SourceUriProperty = AvaloniaProperty.Register<GifImage, Uri>(nameof(SourceUri));
    public static readonly StyledProperty<StretchDirection> StretchDirectionProperty = AvaloniaProperty.Register<GifImage, StretchDirection>(nameof(StretchDirection));
    public static readonly StyledProperty<Stretch> StretchProperty = AvaloniaProperty.Register<GifImage, Stretch>(nameof(Stretch));
    private RenderTargetBitmap _backingRtb;
    private GifInstance _gifInstance;

    static GifImage()
    {
        SourceUriProperty.Changed.Subscribe(new AnonymousObserver<AvaloniaPropertyChangedEventArgs<Uri>>(SourceChanged));
        AffectsRender<GifImage>(SourceUriProperty);
        AffectsArrange<GifImage>(SourceUriProperty);
        AffectsMeasure<GifImage>(SourceUriProperty);
    }

    public Uri SourceUri
    {
        get => GetValue(SourceUriProperty);
        set => SetValue(SourceUriProperty, value);
    }

    public StretchDirection StretchDirection
    {
        get => GetValue(StretchDirectionProperty);
        set => SetValue(StretchDirectionProperty, value);
    }

    public Stretch Stretch
    {
        get => GetValue(StretchProperty);
        set => SetValue(StretchProperty, value);
    }

    /// <inheritdoc/>
    public override void Render(DrawingContext context)
    {
        if (_gifInstance == null)
            return;
                
        if (_gifInstance.GetBitmap() is { } source && _backingRtb is not null)
        {
            using var ctx = _backingRtb.CreateDrawingContext();
            var ts = new Rect(source.Size);
            ((IImage)source).Draw(ctx, ts, ts);
        }

        if (_backingRtb is not null && Bounds is { Width: > 0, Height: > 0 })
        {
            var viewPort = new Rect(Bounds.Size);
            var sourceSize = _backingRtb.Size;

            var scale = Stretch.CalculateScaling(Bounds.Size, sourceSize, StretchDirection);
            var scaledSize = sourceSize * scale;
            var destRect = viewPort
                .CenterRect(new Rect(scaledSize))
                .Intersect(viewPort);
                
            var sourceRect = new Rect(sourceSize)
                .CenterRect(new Rect(destRect.Size / scale));

            ((IImage)_backingRtb).Draw(context, sourceRect, destRect);
        }
            
        Dispatcher.UIThread.Post(InvalidateVisual, DispatcherPriority.Background);
    }

    /// <inheritdoc/>
    protected override Size MeasureOverride(Size availableSize)
        => _backingRtb == null
            ? new Size()
            : Stretch.CalculateSize(availableSize, _backingRtb.Size, StretchDirection);

    /// <inheritdoc/>
    protected override Size ArrangeOverride(Size finalSize)
        => _backingRtb == null
            ? new Size()
            : Stretch.CalculateSize(finalSize, _backingRtb.Size);

    private static void SourceChanged(AvaloniaPropertyChangedEventArgs<Uri> e)
    {
        if (e.Sender is not GifImage image)
        {
            return;
        }

        image._gifInstance?.Dispose();
        image._backingRtb?.Dispose();
        image._backingRtb = null;

        image._gifInstance = new GifInstance();
        image._gifInstance.SetSource(e.NewValue.Value);

        image._backingRtb = new RenderTargetBitmap(image._gifInstance.GifPixelSize, new Vector(96, 96));
    }
}