using System;
using PassMeta.DesktopApp.Common.Abstractions.Entities.PassFile;
using PassMeta.DesktopApp.Common.Enums;

namespace PassMeta.DesktopApp.Common.Models.Entities.PassFile;

/// <summary>
/// Passfile entity.
/// </summary>
public abstract class PassFile : IPassFileWithTimestamps
{
    /// <summary></summary>
    protected PassFile()
    {
        Name ??= "?";
    }

    /// <summary>
    /// Content type.
    /// </summary>
    public abstract PassFileType Type { get; }

    /// <inheritdoc />
    public int Id { get; init; }

    /// <summary>
    /// Identifier of owner user.
    /// </summary>
    public int UserId { get; init; }

    /// <summary>
    /// Name.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Distinctive color (HEX).
    /// </summary>
    public string? Color { get; set; }

    /// <summary>
    /// Timestamp of creation.
    /// </summary>
    public DateTime CreatedOn { get; init; }

    /// <inheritdoc />
    public DateTime InfoChangedOn { get; set; }

    /// <inheritdoc />
    public DateTime VersionChangedOn { get; set; }

    /// <inheritdoc />
    public int Version { get; set; }

    /// <summary>
    /// Origin passfile information from the server
    /// at the moment of the first unsynchronized change, if any.
    /// </summary>
    public IPassFileWithTimestamps? Origin { get; init; }

    /// <summary>
    /// Assigned marks.
    /// </summary>
    public PassFileMark Mark { get; set; }

    /// <inheritdoc />
    public override string ToString()
        => $"<Passfile Id='{Id}', Name='{Name.Replace("'", "\"")}'>";
}

/// <summary>
/// Passfile entity with content.
/// </summary>
public abstract class PassFile<TContent> : PassFile
    where TContent : class
{
    /// <inheritdoc cref="PassFileContent{TData}"/>
    public PassFileContent<TContent> Content { get; set; }
}