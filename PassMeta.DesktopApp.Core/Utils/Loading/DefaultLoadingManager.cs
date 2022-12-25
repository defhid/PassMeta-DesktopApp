using System;
using System.Reactive.Subjects;
using PassMeta.DesktopApp.Common.Abstractions.Utils.Loading;

namespace PassMeta.DesktopApp.Core.Utils.Loading;

/// <inheritdoc />
internal class DefaultLoadingManager : ILoadingManager
{
    private readonly object _lockObject = new();
    private readonly BehaviorSubject<bool> _subject = new(false);
    private int _counter;

    /// <inheritdoc />
    public bool Current => _subject.Value;

    /// <inheritdoc />
    public IObservable<bool> CurrentObservable => _subject;

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
            if (_counter <= 0) return;

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


        public LoadingReleaser(DefaultLoadingManager manager)
        {
            _manager = manager;
        }
        
        public void Dispose()
        {
            _manager.FinishOne();
        }
    }
}