namespace AvaloniaGif;

internal enum BgWorkerState : byte
{
    Null,
    Start,
    Running,
    Paused,
    Complete,
    Dispose
}