namespace AvaloniaGif.Decoding;

public class GifHeader
{
    public bool HasGlobalColorTable;
    public ulong GlobalColorTableCacheId;
    public long HeaderSize;
    public GifRect Dimensions;
}