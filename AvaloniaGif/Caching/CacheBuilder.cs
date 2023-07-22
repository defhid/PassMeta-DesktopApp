using System;

namespace AvaloniaGif.Caching;

/// <summary>
/// Builder for a key-value cache.
/// </summary>
internal class CacheBuilder<TKey, TVal>
    where TVal : class
{
    internal TimeSpan? Expiration { get; private set; }
    internal TimeSpan? PurgeInterval { get; private set; }
    internal bool? DoSlidingExpiration { get; private set; }

    /// <summary>
    /// Sets the maximum time (TTL) that any value will be retained in the cache. This time is
    /// counted from the time when the value was last written (added or updated).
    /// 
    /// If this is null, values will never expire.
    /// </summary>
    /// <param name="expiration">the expiration time, or null if values should never expire</param>
    /// <returns></returns>
    public CacheBuilder<TKey, TVal> WithExpiration(TimeSpan? expiration)
    {
        Expiration = expiration;
        return this;
    }

    public CacheBuilder<TKey, TVal> WithSlidingExpiration()
    {
        DoSlidingExpiration = true;
        return this;
    }

    /// <summary>
    /// Sets the interval in between automatic purges of expired values.
    /// 
    /// If this is not null, then a background task will run at that frequency to sweep the cache for
    /// all expired values.
    /// 
    /// If it is null, expired values will be removed only at the time when you try to access them.
    /// 
    /// This value is ignored if the expiration time (<see cref="WithExpiration(TimeSpan?)"/>) is null.
    /// </summary>
    /// <param name="purgeInterval">the purge interval, or null to turn off automatic purging</param>
    /// <returns></returns>
    public CacheBuilder<TKey, TVal> WithBackgroundPurge(TimeSpan? purgeInterval)
    {
        PurgeInterval = purgeInterval;
        return this;
    }

    /// <summary>
    /// Constructs a cache with the specified properties.
    /// </summary>
    public ICache<TKey, TVal> Build() => new CacheImpl<TKey, TVal>(this);
}