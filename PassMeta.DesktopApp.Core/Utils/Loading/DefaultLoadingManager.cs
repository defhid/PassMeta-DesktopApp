using System;
using System.Reactive.Subjects;
using PassMeta.DesktopApp.Common.Abstractions.Utils.Loading;

namespace PassMeta.DesktopApp.Core.Utils.Loading;

/// <inheritdoc />
public class DefaultLoadingManager : ILoadingManager
{
    private readonly object _lockObject = new();
    private readonly BehaviorSubject<bool> _subject = new(false);
    private int _counter;

    /// <inheritdoc />
    public bool Active => _subject.Value;

    /// <inheritdoc />
    public IObservable<bool> ActiveObservable => _subject;

    /// <inheritdoc />
    public IDisposable Begin()
    {
        StartOne();
        return new LoadingReleaser(this);
    }

    private void StartOne()
    {
        lock (_lockObject)
        {
            ++_counter;
            if (_counter == 1)
            {
                _subject.OnNext(true);
            }
        }
    }

    private void FinishOne()
    {
        lock (_lockObject)
        {
            if (_counter <= 0)
            {
                throw new InvalidOperationException("Cannot finish extra loading counter");
            }

            --_counter;

            if (_counter == 0)
            {
                _subject.OnNext(false);
            }
        }
    }
        
    private class LoadingReleaser : IDisposable
    {
        private readonly DefaultLoadingManager _manager;
        private bool _disposed;

        public LoadingReleaser(DefaultLoadingManager manager)
        {
            _manager = manager;
        }

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            _manager.FinishOne();
            _disposed = true;
        }
    }
}