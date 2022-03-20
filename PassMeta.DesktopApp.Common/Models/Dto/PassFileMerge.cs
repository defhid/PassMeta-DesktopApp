namespace PassMeta.DesktopApp.Common.Models.Dto
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using Entities;

    /// <summary>
    /// Passfile merge DTO.
    /// </summary>
    public class PassFileMerge
    {
        /// <summary>
        /// Local and remote passfile versions.
        /// </summary>
        public readonly (int Local, int Remote, int Splitting) Versions;
        
        /// <summary>
        /// Local and remote passfile "changed on" timestamps.
        /// </summary>
        public readonly (DateTime Local, DateTime Remote, DateTime Splitting) VersionsChangedOn;

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
        public readonly List<Conflict> Conflicts = new();

        /// <summary></summary>
        public PassFileMerge(PassFile localPassFile, PassFile remotePassFile)
        {
            Versions = (localPassFile.Version, remotePassFile.Version, localPassFile.Origin!.Version);
            VersionsChangedOn = (localPassFile.VersionChangedOn, remotePassFile.VersionChangedOn, localPassFile.Origin!.VersionChangedOn);
            LocalSections = localPassFile.Data!.Select(section => section.Copy()).ToList();
            RemoteSections = remotePassFile.Data!.Select(section => section.Copy()).ToList();
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