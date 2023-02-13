using System;
using System.Reactive.Subjects;
using PassMeta.DesktopApp.Common.Abstractions.Utils.Loading;

namespace PassMeta.DesktopApp.Core.Utils.Loading;

/// <inheritdoc />
internal class DefaultLoadingManager : ILoadingManager
{
    private readonly object _lockObject = new();
    private readonly BehaviorSubject<bool> _subject = new(false);
    private readonly Func<string> _nameGetter;
    private int _counter;

    public DefaultLoadingManager(Func<string> nameGetter)
    {
        _nameGetter = nameGetter;
    }

    /// <inheritdoc />
    public string Name => _nameGetter();

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