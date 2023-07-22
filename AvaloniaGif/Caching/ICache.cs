using System;

namespace AvaloniaGif.Caching;

/// <summary>
/// Interface for a key-value cache.
/// </summary>
/// <typeparam name="TKey">the key type</typeparam>
/// <typeparam name="TVal">the value type</typeparam>
internal interface ICache<in TKey, TVal> : IDisposable
{
    /// <summary>
    /// Attempts to get the value associated with the given key.
    /// 
    /// In a read-through cache, if there is no cached value for the key, the cache will call
    /// the loader function to provide a value; thus, a value is always available.
    /// 
    /// If it is not a read-through cache and no value is available, the cache does not throw
    /// an exception (unlike IDictionary). Instead, it returns the default value for type V
    /// (null, if it is a reference type).
    /// </summary>
    /// <param name="key">the key</param>
    /// <returns>the associated value, or <code>default(V)</code></returns>
    TVal Get(TKey key);

    /// <summary>
    /// Attempts to get the value associated with the given key. If successful, sets
    /// <code>value</code> to the value and returns true; otherwise, sets <code>value</code>
    /// to <code>default(V)</code> and returns false.
    /// 
    /// In a read-through cache, if there is no cached value for the key, the cache will call
    /// the loader function to provide a value; thus, it will always return true.
    /// </summary>
    /// <returns>true if there is a value</returns>
    bool TryGetValue(TKey key, out TVal value);

    /// <summary>
    /// Stores a value associated with the given key.
    /// 
    /// Note that any value of type V can be cached, including null for reference types.
    /// </summary>
    /// <param name="key">the key</param>
    /// <param name="value">the value</param>
    void Set(TKey key, TVal value);

    /// <summary>
    /// Removes the value associated with the given key, if any.
    /// </summary>
    /// <param name="key">the key</param>
    void Remove(TKey key);

    /// <summary>
    /// Removes all cached values.
    /// </summary>
    void Clear();
}