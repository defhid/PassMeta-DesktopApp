using AvaloniaGif.Decoding;
using System;
using System.IO;
using Avalonia;
using Avalonia.Media.Imaging;
using Avalonia.Platform;

namespace AvaloniaGif;

public sealed class GifInstance : IDisposable
{
    private GifDecoder _gifDecoder;
    private GifBackgroundWorker _bgWorker;
    private WriteableBitmap _targetBitmap;
    private bool _hasNewFrame;
    private bool _isDisposed;
    private Stream _stream;

    public void SetSource(Uri uri)
    {
        if (uri == null)
        {
            throw new InvalidDataException("Missing valid URI or Stream.");
        }

        if (uri.OriginalString.Trim().StartsWith("resm"))
        {
            _stream = AssetLoader.Open(uri);
        }

        _gifDecoder = new GifDecoder(_stream);
        _bgWorker = new GifBackgroundWorker(_gifDecoder);

        GifPixelSize = new PixelSize(_gifDecoder.Header.Dimensions.Width, _gifDecoder.Header.Dimensions.Height);

        _targetBitmap = new WriteableBitmap(GifPixelSize, new Vector(96, 96), PixelFormat.Bgra8888, AlphaFormat.Opaque);
        _bgWorker.CurrentFrameChanged += FrameChanged;

        Run();
    }

    public PixelSize GifPixelSize { get; private set; }

    public WriteableBitmap GetBitmap()
    {
        if (!_hasNewFrame)
        {
            return null;
        }

        _hasNewFrame = false;
        return _targetBitmap;
    }

    private void FrameChanged()
    {
        if (_isDisposed) return;
        _hasNewFrame = true;

        using var lockedBitmap = _targetBitmap.Lock();
        _gifDecoder?.WriteBackBufToFb(lockedBitmap.Address);
    }

    private void Run()
    {
        if (!_stream.CanSeek)
        {
            throw new ArgumentException("The stream is not seekable");
        }

        _bgWorker?.SendCommand(BgWorkerCommand.Play);
    }

    public void Dispose()
    {
        _isDisposed = true;
        _bgWorker?.SendCommand(BgWorkerCommand.Dispose);
        _targetBitmap?.Dispose();
    }
}