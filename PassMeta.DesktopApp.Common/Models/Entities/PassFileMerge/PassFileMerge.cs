using System;

namespace PassMeta.DesktopApp.Common.Models.Entities.PassFileMerge;

/// <summary>
/// Passfile content merge.
/// </summary>
public abstract class PassFileMerge<TContent>
    where TContent : class, new()
{
    /// <summary>
    /// Local and remote passfile content versions.
    /// </summary>
    public readonly (int Local, int Remote, int Branching) Versions;

    /// <summary>
    /// Local and remote passfile content "changed on" timestamps.
    /// </summary>
    public readonly (DateTime Local, DateTime Remote, DateTime Branching) VersionsChangedOn;

    /// <summary>
    /// Result content.
    /// </summary>
    public readonly TContent Result;

    /// <summary></summary>
    protected PassFileMerge(PassFile.PassFile localPassFile, PassFile.PassFile remotePassFile)
    {
        Result = new TContent();
        Versions = (
            localPassFile.Version,
            remotePassFile.Version,
            localPassFile.Origin!.Version);
        VersionsChangedOn = (
            localPassFile.VersionChangedOn,
            remotePassFile.VersionChangedOn,
            localPassFile.Origin!.VersionChangedOn);
    }
}