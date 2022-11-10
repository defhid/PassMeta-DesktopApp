namespace PassMeta.DesktopApp.Core
{
    using Common.Enums;

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
            public static string GetInfo() => "info";

            /// Get OK response.
            /// 
            public static string GetCheck() => "check";
        }

        /// <summary>
        /// User controllers.
        /// </summary>
        public static class User
        {
            /// Create a new user.
            /// 
            public static string Post() => "users/new";

            /// Get current user.
            /// 
            public static string GetMe() => "users/me";

            /// Edit current user.
            /// 
            public static string PatchMe() => "users/me";
        }

        /// <summary>
        /// Auth controllers.
        /// </summary>
        public static class Auth
        {
            /// Sign in.
            /// 
            public static string PostSignIn() => "auth/sign-in";

            /// Reset all current user sessions.
            /// 
            public static string PostResetAll() => "auth/reset/all";

            /// Reset all current user sessions except this one.
            /// 
            public static string PostResetAllExceptMe() => "auth/reset/all-except-me";
        }

        /// <summary>
        /// Passfile controllers.
        /// </summary>
        public static class PassFile
        {
            /// Create a new passfile.
            /// 
            public static string Post() => "passfiles/new";

            /// Get passfile list.
            /// 
            public static string GetList(PassFileType ofType) => $"passfiles?type_id={(int)ofType}";

            /// Get passfile.
            ///
            public static string Get(int passFileId) => $"passfiles/{passFileId}";

            /// Edit passfile.
            ///
            public static string Patch(int passFileId) => $"passfiles/{passFileId}";

            /// Delete passfile.
            ///
            public static string Delete(int passFileId) => $"passfiles/{passFileId}";

            /// Create a new passfile version content.
            ///
            public static string PostVersion(int passFileId) => $"passfiles/{passFileId}/versions/new";

            /// Get passfile versions.
            ///
            public static string GetVersionList(int passFileId) => $"passfiles/{passFileId}/versions";

            /// Get passfile version content.
            ///
            public static string GetVersion(int passFileId, int version) => $"passfiles/{passFileId}/versions/{version}";
        }

        /// <summary>
        /// History controllers.
        /// </summary>
        public static class History
        {
            /// Get history kinds.
            ///
            public static string GetKinds() => "history/kinds";

            /// Get paged history.
            ///
            public static string Get() => "history";
        }
    }
}