using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace PassMeta.DesktopApp.Core.Utils;

/// <summary>
/// Methods for working with cookies.
/// </summary>
public static class CookieHelper
{
    /// <summary>
    /// Get <paramref name="freshCookies"/> with absent cookies
    /// from <paramref name="currentCookies"/>.
    /// </summary>
    public static IEnumerable<Cookie> JoinCookies(IEnumerable<Cookie> currentCookies, IEnumerable<Cookie> freshCookies)
    {
        var result = new HashSet<Cookie>(freshCookies, CookieNameComparer.Instance);

        foreach (var currentCookie in currentCookies)
        {
            _ = result.Add(currentCookie);
        }

        return result.Select(SafeCopy);
    }

    /// <summary>
    /// Build <see cref="CookieContainer"/> by given <paramref name="actualCookies"/>.
    /// </summary>
    public static void RefillCookieContainer(CookieContainer container, IEnumerable<Cookie> actualCookies)
    {
        foreach (var cookie in container.GetAllCookies().Cast<Cookie>())
        {
            cookie.Expired = true;
        }
            
        foreach (var cookie in actualCookies)
        {
            container.Add(SafeCopy(cookie));
        }
    }

    private static Cookie SafeCopy(Cookie origin) => new(origin.Name, origin.Value, null, origin.Domain);

    private class CookieNameComparer : IEqualityComparer<Cookie>
    {
        public static readonly CookieNameComparer Instance = new();

        public bool Equals(Cookie? x, Cookie? y)
            => ReferenceEquals(x, y) ||
               !ReferenceEquals(x, null) && !ReferenceEquals(y, null) && x.Name == y.Name;

        public int GetHashCode(Cookie obj) => obj.Name.GetHashCode();
    }
}