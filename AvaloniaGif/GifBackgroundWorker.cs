using System;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Collections.Generic;
using AvaloniaGif.Decoding;
using System.Linq;

namespace AvaloniaGif;

internal sealed class GifBackgroundWorker
{
    private static readonly Stopwatch Timer = Stopwatch.StartNew();
    private readonly GifDecoder _gifDecoder;
    private readonly object _lockObj;
    private readonly Queue<BgWorkerCommand> _cmdQueue;
    private readonly List<ulong> _colorTableIdList;
    private readonly GifRepeatBehavior _repeatBehavior;
    private volatile bool _shouldStop;
    private int _iterationCount;
    private int _currentIndex;
    private BgWorkerState _state;
    public Action CurrentFrameChanged;

    private void ResetPlayVars()
    {
        _iterationCount = 0;

        if (_currentIndex == -1)
        {
            return;
        }

        lock (_lockObj)
        {
            InternalSeek(-1);
        }
    }

    private void RefreshColorTableCache()
    {
        foreach (var cacheId in _colorTableIdList)
        {
            GifDecoder.GlobalColorTableCache.TryGetValue(cacheId, out _);
        }
    }

    private void InternalSeek(int value)
    {
        var lowerBound = 0;

        // Skip already rendered frames if the seek position is above the previous frame index.
        if (value > _currentIndex)
        {
            // Only render the new seeked frame if the delta
            // seek position is just 1 frame.
            if (value - _currentIndex == 1)
            {
                _gifDecoder.RenderFrame(value);
                SetIndexVal(value);
                return;
            }

            lowerBound = _currentIndex;
        }

        for (var fI = lowerBound; fI <= value; fI++)
        {
            var targetFrame = _gifDecoder.Frames[fI];

            // Ignore frames with restore disposal method except the current one.
            if (fI != value & targetFrame.FrameDisposalMethod == FrameDisposal.Restore)
            {
                continue;
            }

            _gifDecoder.RenderFrame(fI);
        }

        SetIndexVal(value);
    }

    private void SetIndexVal(int value)
    {
        _currentIndex = value;

        if (_state == BgWorkerState.Complete)
        {
            _state = BgWorkerState.Paused;
            _iterationCount = 0;
        }

        CurrentFrameChanged?.Invoke();
    }

    public GifBackgroundWorker(GifDecoder gifDecode)
    {
        _gifDecoder = gifDecode;
        _lockObj = new object();
        _repeatBehavior = new GifRepeatBehavior { LoopForever = true };
        _cmdQueue = new Queue<BgWorkerCommand>();

        // Save the color table cache ID's to refresh them on cache while
        // the image is either stopped/paused.
        _colorTableIdList = _gifDecoder.Frames
            .Where(p => p.IsLocalColorTableUsed)
            .Select(p => p.LocalColorTableCacheId)
            .ToList();

        if (_gifDecoder.Header.HasGlobalColorTable)
            _colorTableIdList.Add(_gifDecoder.Header.GlobalColorTableCacheId);

        ResetPlayVars();

        Task.Factory.StartNew(MainLoop, CancellationToken.None, TaskCreationOptions.LongRunning, TaskScheduler.Current);
    }

    public void SendCommand(BgWorkerCommand cmd)
    {
        lock (_lockObj)
        {
            _cmdQueue.Enqueue(cmd);
        }
    }

    private void MainLoop()
    {
        while (true)
        {
            if (_shouldStop)
            {
                DoDispose();
                break;
            }

            CheckCommands();
            DoStates();
        }
    }

    private void DoStates()
    {
        switch (_state)
        {
            case BgWorkerState.Null:
                Thread.Sleep(40);
                break;
            case BgWorkerState.Paused:
                RefreshColorTableCache();
                Thread.Sleep(60);
                break;
            case BgWorkerState.Start:
                _state = BgWorkerState.Running;
                break;
            case BgWorkerState.Running:
                WaitAndRenderNext();
                break;
            case BgWorkerState.Complete:
                RefreshColorTableCache();
                Thread.Sleep(60);
                break;
        }
    }

    private void CheckCommands()
    {
        BgWorkerCommand cmd;

        lock (_lockObj)
        {
            if (_cmdQueue.Count <= 0) return;
            cmd = _cmdQueue.Dequeue();
        }

        switch (cmd)
        {
            case BgWorkerCommand.Dispose:
            {
                DoDispose();
                break;
            }
            case BgWorkerCommand.Play:
            {
                switch (_state)
                {
                    case BgWorkerState.Null:
                        _state = BgWorkerState.Start;
                        break;
                    case BgWorkerState.Paused:
                        _state = BgWorkerState.Running;
                        break;
                    case BgWorkerState.Complete:
                        ResetPlayVars();
                        _state = BgWorkerState.Start;
                        break;
                }
                break;
            }
            case BgWorkerCommand.Pause:
            {
                if (_state is BgWorkerState.Running)
                {
                    _state = BgWorkerState.Paused;
                }
                break;
            }
        }
    }

    private void DoDispose()
    {
        _state = BgWorkerState.Dispose;
        _shouldStop = true;
        _gifDecoder.Dispose();
    }

    private void WaitAndRenderNext()
    {
        if (!_repeatBehavior.LoopForever & _iterationCount > 0)
        {
            _state = BgWorkerState.Complete;
            return;
        }

        _currentIndex = (_currentIndex + 1) % _gifDecoder.Frames.Count;

        CurrentFrameChanged?.Invoke();

        var targetDelay = _gifDecoder.Frames[_currentIndex].FrameDelay;

        var t1 = Timer.Elapsed;

        _gifDecoder.RenderFrame(_currentIndex);

        var t2 = Timer.Elapsed;
        var delta = t2 - t1;

        if (delta > targetDelay) return;
        Thread.Sleep(targetDelay - delta);

        if (!_repeatBehavior.LoopForever & _currentIndex == 0)
            _iterationCount++;
    }

    ~GifBackgroundWorker()
    {
        DoDispose();
    }
}