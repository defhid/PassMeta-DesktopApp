namespace PassMeta.DesktopApp.Common.Models.Dto
{
    using System.Collections.Generic;
    using System.Diagnostics;
    using Entities;

    /// <summary>
    /// Passfile merge DTO.
    /// </summary>
    public class PassFileMerge
    {
        /// <summary>
        /// Sections from local passfile version.
        /// </summary>
        public readonly List<PassFile.Section> LocalSections;

        /// <summary>
        /// Sections from remote passfile version.
        /// </summary>
        public readonly List<PassFile.Section> RemoteSections;

        /// <summary>
        /// Conflicts between <see cref="LocalSections"/> and <see cref="RemoteSections"/>.
        /// </summary>
        public readonly List<PassFile.Section> Conflicts = new();

        /// <summary></summary>
        public PassFileMerge(List<PassFile.Section> localSections, List<PassFile.Section> remoteSections)
        {
            LocalSections = localSections;
            RemoteSections = remoteSections;
        }

        /// <summary>
        /// Merge conflict.
        /// </summary>
        public class Conflict
        {
            /// Local section.
            public readonly PassFile.Section? Local;

            /// Remote section.
            public readonly PassFile.Section? Remote;

            /// <summary></summary>
            public Conflict(PassFile.Section? local, PassFile.Section? remote)
            {
                Debug.Assert(local is not null || remote is not null);
                Local = local;
                Remote = remote;
            }
        }
    }
}