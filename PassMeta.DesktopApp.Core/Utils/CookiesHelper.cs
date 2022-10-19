namespace PassMeta.DesktopApp.Core.Utils
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;

    /// <summary>
    /// Methods for working with cookies.
    /// </summary>
    public static class CookiesHelper
    {
        /// <summary>
        /// Add absent cookies to <paramref name="cookies"/>
        /// or replace existing ones
        /// from <paramref name="freshCookies"/>.
        /// </summary>
        /// <returns>Changed.</returns>
        public static bool RefreshCookies(List<Cookie> cookies, IEnumerable<Cookie> freshCookies)
        {
            var changed = false;
            
            foreach (var fresh in freshCookies.Reverse())
            {
                var currentIndex = cookies.FindIndex(c => c.Name == fresh.Name);
                if (currentIndex < 0)
                {
                    cookies.Add(fresh);
                    changed = true;
                }
                else
                {
                    cookies[currentIndex] = fresh;
                    changed = true;
                }
            }

            return changed;
        }

        /// <summary>
        /// Build <see cref="CookieContainer"/> by given <paramref name="cookies"/>.
        /// </summary>
        public static CookieContainer BuildCookieContainer(IEnumerable<Cookie> cookies)
        {
            var container = new CookieContainer();
            
            foreach (var cookie in cookies)
            {
                // new cookie to attach to requests correctly
                container.Add(new Cookie(cookie.Name, cookie.Value, null, cookie.Domain));
            }

            return container;
        }
    }
}