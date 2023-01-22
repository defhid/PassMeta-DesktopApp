using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace PassMeta.DesktopApp.Common.Collections;

/// <summary>
/// LRU cache.
/// </summary>
public class LruCache<TKey, TValue> : IEnumerable<KeyValuePair<TKey, TValue>>
    where TKey : notnull
{
    private readonly int _capacity;
    private readonly Dictionary<TKey, LinkedListNode<KeyValuePair<TKey, TValue>>> _cacheMap = new();
    private readonly LinkedList<KeyValuePair<TKey, TValue>> _lruList = new();

    /// <summary></summary>
    public LruCache(int capacity)
    {
        _capacity = capacity;
    }

    /// <summary></summary>
    public bool TryGet(TKey key, [MaybeNullWhen(false)] out TValue value)
    {
        if (_cacheMap.TryGetValue(key, out var node))
        {
            _lruList.Remove(node);
            _lruList.AddLast(node);
            value = node.Value.Value;
            return true;
        }

        value = default;
        return false;
    }

    /// <summary></summary>
    public void Set(TKey key, TValue value)
    {
        if (_cacheMap.TryGetValue(key, out var existingNode))
        {
            _lruList.Remove(existingNode);
        }
        else if (_cacheMap.Count >= _capacity)
        {
            _cacheMap.Remove(_lruList.First!.Value.Key);
            _lruList.RemoveFirst();
        }

        var node = new LinkedListNode<KeyValuePair<TKey, TValue>>(new KeyValuePair<TKey, TValue>(key, value));
        _cacheMap[key] = node;
        _lruList.AddLast(node);
    }

    /// <inheritdoc />
    public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator() => _lruList.GetEnumerator();

    /// <inheritdoc />
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}