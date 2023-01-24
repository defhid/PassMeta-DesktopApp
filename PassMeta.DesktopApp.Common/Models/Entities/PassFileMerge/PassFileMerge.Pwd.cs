using System.Collections.Generic;
using System.Diagnostics;
using PassMeta.DesktopApp.Common.Enums;
using PassMeta.DesktopApp.Common.Models.Entities.PassFile.Data;

namespace PassMeta.DesktopApp.Common.Models.Entities.PassFileMerge;

/// <summary>
/// Passfile of <see cref="PassFileType.Pwd"/> type merge.
/// </summary>
public class PwdPassFileMerge : PassFileMerge<List<PwdSection>>
{
    /// <summary>
    /// Conflicts between sections from remote and local versions.
    /// </summary>
    public readonly List<Conflict> Conflicts = new();

    /// <summary></summary>
    public PwdPassFileMerge(PassFile.PassFile localPassFile, PassFile.PassFile remotePassFile)
        : base(localPassFile, remotePassFile)
    {
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