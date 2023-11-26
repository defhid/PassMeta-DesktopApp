using System;
using System.Collections.Generic;

namespace PassMeta.DesktopApp.Common.Extensions;

/// <summary>
/// Extension methods for <see cref="IReadOnlyList{T}"/>
/// </summary>
public static class ReadOnlyListExtensions
{
    /// <summary>
    /// Get index of <paramref name="element"/> in <paramref name="list"/>.
    /// </summary>
    /// <returns>Positive index or -1.</returns>
    public static int IndexOf<TSource>(this IReadOnlyList<TSource> list, TSource? element)
    {
        var count = list.Count;
            
        for (var i = 0; i < count; ++i)
        {
            if (Equals(list[i], element)) return i;
        }

        return -1;
    }
        
    /// <summary>
    /// Get index by pattern.
    /// </summary>
    /// <returns>Positive index or -1.</returns>
    public static int FindIndex<TSource>(this IReadOnlyList<TSource> list, Func<TSource, bool> pattern)
    {
        var count = list.Count;
            
        for (var i = 0; i < count; ++i)
        {
            if (pattern(list[i])) return i;
        }

        return -1;
    }
}