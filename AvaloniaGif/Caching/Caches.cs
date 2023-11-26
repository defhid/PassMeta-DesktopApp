namespace AvaloniaGif.Caching;

/// <summary>
/// Methods for building caches.
/// </summary>
internal static class Caches
{
    /// <summary>
    /// Starts constructing a key-value cache.
    /// </summary>
    public static CacheBuilder<TKey, TVal> KeyValue<TKey, TVal>()
        where TVal : class 
        => new();
}