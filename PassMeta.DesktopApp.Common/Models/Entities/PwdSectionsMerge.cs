using System;
using System.Collections.Generic;
using System.Diagnostics;
using PassMeta.DesktopApp.Common.Enums;
using PassMeta.DesktopApp.Common.Models.Entities.PassFile.Data;

namespace PassMeta.DesktopApp.Common.Models.Entities;

/// <summary>
/// Passfile of <see cref="PassFileType.Pwd"/> type merge.
/// </summary>
public class PwdSectionsMerge
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
    /// Result passfile sections.
    /// </summary>
    public readonly List<PwdSection> ResultSections = new();

    /// <summary>
    /// Conflicts between sections from remote and local versions.
    /// </summary>
    public readonly List<Conflict> Conflicts = new();

    /// <summary></summary>
    public PwdSectionsMerge(PassFile localPassFile, PassFile remotePassFile)
    {
        Versions = (localPassFile.Version, remotePassFile.Version, localPassFile.Origin!.Version);
        VersionsChangedOn = (localPassFile.VersionChangedOn, remotePassFile.VersionChangedOn, localPassFile.Origin!.VersionChangedOn);
    }

    /// <summary>
    /// Merge conflict.
    /// </summary>
    public class Conflict
    {
        /// Local section.
        public readonly PwdSection? Local;

        /// Remote section.
        public readonly PwdSection? Remote;

        /// <summary></summary>
        public Conflict(PwdSection? local, PwdSection? remote)
        {
            Debug.Assert(local is not null || remote is not null);
            Local = local;
            Remote = remote;
        }
    }
}