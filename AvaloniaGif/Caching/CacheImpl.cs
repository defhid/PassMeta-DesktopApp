/*
 This source file is derived from https://github.com/launchdarkly/dotnet-cache/
 Under the terms of Apache 2.0 License.
 */

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace AvaloniaGif.Caching;

/// <summary>
/// A concurrent in-memory cache with optional read-through behavior, an optional TTL, and the
/// ability to explicitly set values. Expired entries are purged by a background task.
/// 
/// A cache hit, or a miss without read-through, requires only one read lock. A cache miss
/// with read-through requires read and write locks on the cache, and then a write lock on the
/// individual entry.
/// 
/// Loading requests are coalesced, i.e. if multiple threads request the same key at the same
/// time, only one will call the loader function and the others will wait on it.
///
/// Null values are allowed.
/// </summary>
internal sealed class CacheImpl<TKey, TVal> : ICache<TKey, TVal>
    where TVal : class
{
    private readonly TimeSpan? _expiration;
    private readonly TimeSpan? _purgeInterval;
    private readonly bool _doSlidingExp;
    private readonly int? _maxEntries;
    private readonly IDictionary<TKey, CacheEntry<TKey, TVal>> _entries;
    private readonly LinkedList<TKey> _keysInCreationOrder = new();
    private readonly ReaderWriterLockSlim _wholeCacheLock = new();
    private volatile bool _disposed;

    public CacheImpl(CacheBuilder<TKey, TVal> builder)
    {
        _entries = new Dictionary<TKey, CacheEntry<TKey, TVal>>();

        _maxEntries = null;
        _expiration = builder.Expiration;
        _purgeInterval = builder.PurgeInterval;
        _doSlidingExp = builder.DoSlidingExpiration ?? false;
            
        if (_expiration.HasValue && _purgeInterval.HasValue)
        {
            var interval = _purgeInterval.Value;
            Task.Run(() => PurgeExpiredEntriesAsync(interval));
        }
    }

    public bool TryGetValue(TKey key, out TVal value)
    {
        _wholeCacheLock.EnterReadLock();
        bool entryExists;
        CacheEntry<TKey, TVal> entry;
        try
        {
            entryExists = _entries.TryGetValue(key, out entry);
        }
        finally
        {
            _wholeCacheLock.ExitReadLock();
        }

        if (entryExists)
        {
            // Reset entry expiration when sliding expiration is enabled.
            if (_doSlidingExp & _expiration.HasValue)
                entry.ExpirationTime = DateTime.Now.Add(_expiration.Value);

            if (entry.IsExpired())
            {
                // If _purgeInterval is non-null then we will leave it for the background task to handle.
                // Likewise, if we have a loader function, then we don't need to explicitly remove it here
                // because it will get overwritten further down. But if we don't have a loader and we
                // don't have a background task, then we need to remove the expired entry now.
                if (_purgeInterval == null)
                {
                    Remove(key);
                }
            }
            else
            {
                value = entry.Value;
                return true;
            }
        }

        value = default;
        return false;
    }

    public TVal Get(TKey key)
    {
        TryGetValue(key, out var value);
        return value;
    }

    public void Set(TKey key, TVal value)
    {
        _wholeCacheLock.EnterWriteLock();
        try
        {
            if (_entries.TryGetValue(key, out var oldEntry))
            {
                _keysInCreationOrder.Remove(oldEntry.Node);
            }

            DateTime? expTime = null;
            if (_expiration.HasValue)
            {
                expTime = DateTime.Now.Add(_expiration.Value);
            }

            var node = new LinkedListNode<TKey>(key);
            var entry = new CacheEntry<TKey, TVal>(expTime, node)
            {
                Value = value
            };
            _entries[key] = entry;
            _keysInCreationOrder.AddLast(node);
            PurgeExcessEntries();
        }
        finally
        {
            _wholeCacheLock.ExitWriteLock();
        }
    }

    public void Remove(TKey key)
    {
        _wholeCacheLock.EnterWriteLock();
        try
        {
            if (_entries.TryGetValue(key, out var entry))
            {
                _entries.Remove(key);
                _keysInCreationOrder.Remove(entry.Node);
            }
        }
        finally
        {
            _wholeCacheLock.ExitWriteLock();
        }
    }

    public void Clear()
    {
        _wholeCacheLock.EnterWriteLock();
        try
        {
            _entries.Clear();
        }
        finally
        {
            _wholeCacheLock.ExitWriteLock();
        }
    }

    public void Dispose()
    {
        _disposed = true;
    }

    private void PurgeExcessEntries()
    {
        // must be called under a write lock
        if (!_wholeCacheLock.IsWriteLockHeld)
        {
            return;
        }

        if (_maxEntries != null)
        {
            while (_entries.Count > _maxEntries.Value)
            {
                var first = _keysInCreationOrder.First;
                _keysInCreationOrder.RemoveFirst();
                _entries.Remove(first.Value);
            }
        }
    }

    private void PurgeExpiredEntries()
    {
        _wholeCacheLock.EnterWriteLock();
        try
        {
            while (_keysInCreationOrder.Count > 0 &&
                   _entries[_keysInCreationOrder.First.Value].IsExpired())
            {
                _entries.Remove(_keysInCreationOrder.First.Value);
                _keysInCreationOrder.RemoveFirst();
            }
        }
        finally
        {
            _wholeCacheLock.ExitWriteLock();
        }
    }

    private async Task PurgeExpiredEntriesAsync(TimeSpan interval)
    {
        while (!_disposed)
        {
            await Task.Delay(interval);
            PurgeExpiredEntries();
        }
    }
}

internal class CacheEntry<TKey, TVal>
    where TVal : class
{
    public DateTime? ExpirationTime;
    public readonly LinkedListNode<TKey> Node;
    public volatile TVal Value;

    public CacheEntry(DateTime? expirationTime, LinkedListNode<TKey> node)
    {
        ExpirationTime = expirationTime;
        Node = node;
    }

    public bool IsExpired()
    {
        return ExpirationTime.HasValue && ExpirationTime.Value.CompareTo(DateTime.Now) <= 0;
    }
}
