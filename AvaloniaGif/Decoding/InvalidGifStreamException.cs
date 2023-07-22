using System;

namespace AvaloniaGif.Decoding;

[Serializable]
public class InvalidGifStreamException : Exception
{
    public InvalidGifStreamException()
    {
    }

    public InvalidGifStreamException(string message) : base(message)
    {
    }
}