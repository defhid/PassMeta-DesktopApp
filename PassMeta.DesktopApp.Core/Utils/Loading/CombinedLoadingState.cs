using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Subjects;
using PassMeta.DesktopApp.Common.Abstractions.Utils.Loading;

namespace PassMeta.DesktopApp.Core.Utils.Loading;

internal class CombinedLoadingState : ILoadingState
{
    private readonly bool[] _loadings;
    private readonly BehaviorSubject<bool> _subject;

    public CombinedLoadingState(IEnumerable<ILoadingState> states)
    {
        var stateList = states.ToList();
            
        _loadings = new bool[stateList.Count];
        _subject = new BehaviorSubject<bool>(false);

        for (var i = 0; i < stateList.Count; ++i)
        {
            Subscribe(stateList[i], i);
        }
    }

    public bool Current => _loadings.Any(x => x);

    public IObservable<bool> CurrentObservable => _subject;

    private void Subscribe(ILoadingState state, int index)
    {
        state.CurrentObservable.Subscribe(x =>
        {
            lock (_loadings)
            {
                _loadings[index] = x;

                var current = Current;
                if (current != _subject.Value)
                {
                    _subject.OnNext(current);
                }
            }
        });
    }
}