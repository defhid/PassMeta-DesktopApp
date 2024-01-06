using System;
using System.Collections.Generic;
using System.Net.Http;
using PassMeta.DesktopApp.Common.Abstractions.Utils.PassMetaClient;
using PassMeta.DesktopApp.Common.Enums;

namespace PassMeta.DesktopApp.Core;

/// <summary>
/// PassMeta API description.
/// </summary>
public static class PassMetaApi
{
    /// <summary>
    /// General controllers.
    /// </summary>
    public static class General
    {
        /// Get current session and server information.
        /// 
        public static IHttpRequestBase GetInfo() => _Get("info");

        /// Get OK response.
        /// 
        public static IHttpRequestBase GetCheck() => _Get("check");
    }

    /// <summary>
    /// User controllers.
    /// </summary>
    public static class User
    {
        /// Create a new user.
        /// 
        public static IHttpRequestWithBodySupportBase Post() => _Post("users/new");

        /// Get current user.
        /// 
        public static IHttpRequestBase GetMe() => _Get("users/me");

        /// Edit current user.
        /// 
        public static IHttpRequestWithBodySupportBase PatchMe() => _Patch("users/me");
    }

    /// <summary>
    /// Auth controllers.
    /// </summary>
    public static class Auth
    {
        /// Sign in.
        /// 
        public static IHttpRequestWithBodySupportBase PostLogIn() => _Post("auth/sign-in");

        /// Reset all current user sessions.
        /// 
        public static IHttpRequestBase PostResetAll() => _Post("auth/reset/all");

        /// Reset all current user sessions except this one.
        /// 
        public static IHttpRequestBase PostResetAllExceptMe() => _Post("auth/reset/all-except-me");
    }

    /// <summary>
    /// Passfile controllers.
    /// </summary>
    public static class PassFile
    {
        /// Create a new passfile.
        /// 
        public static IHttpRequestWithBodySupportBase Post() => _Post("passfiles/new");

        /// Get passfile list.
        /// 
        public static IHttpRequestBase GetList(PassFileType ofType) => _Get($"passfiles?typeId={(int)ofType}");

        /// Get passfile.
        ///
        public static IHttpRequestBase Get(long passFileId) => _Get($"passfiles/{passFileId}");

        /// Edit passfile.
        ///
        public static IHttpRequestWithBodySupportBase Patch(long passFileId) => _Patch($"passfiles/{passFileId}");

        /// Delete passfile.
        ///
        public static IHttpRequestWithBodySupportBase Delete(long passFileId) => _Delete($"passfiles/{passFileId}");

        /// Create a new passfile version content.
        ///
        public static IHttpRequestWithBodySupportBase PostVersion(long passFileId) => _Post($"passfiles/{passFileId}/versions/new");

        /// Get passfile versions.
        ///
        public static IHttpRequestBase GetVersionList(long passFileId) => _Get($"passfiles/{passFileId}/versions");

        /// Get passfile version content.
        ///
        public static IHttpRequestBase GetVersion(long passFileId, int version) => _Get($"passfiles/{passFileId}/versions/{version}");
    }

    /// <summary>
    /// History controllers.
    /// </summary>
    public static class History
    {
        /// Get history kinds.
        ///
        public static IHttpRequestBase GetKinds() => _Get("history/kinds");

        /// Get paged history.
        ///
        public static IHttpRequestBase GetList(DateTime month, int pageSize, int pageIndex, ICollection<int>? kinds = null)
        {
            var kindsFilter = kinds?.Count > 0 ? "&kind=" + string.Join(",", kinds) : string.Empty;

            return _Get($"history/pages/{pageIndex}?month={month:yyyy-MM}-01&pageSize={pageSize}{kindsFilter}");
        }
    }
        
    private static HttpRequestBase _Get(string url) => new(HttpMethod.Get, url);

    private static HttpRequestBase _Post(string url) => new(HttpMethod.Post, url);

    private static HttpRequestBase _Patch(string url) => new(HttpMethod.Patch, url);

    private static HttpRequestBase _Delete(string url) => new(HttpMethod.Delete, url);
}

internal class HttpRequestBase(HttpMethod method, string url) : IHttpRequestWithBodySupportBase
{
    public string Url { get; } = url;
    public HttpMethod Method { get; } = method;
}