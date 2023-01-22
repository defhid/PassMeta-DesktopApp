using System;
using PassMeta.DesktopApp.Common.Enums;

namespace PassMeta.DesktopApp.Common.Abstractions.Entities.PassFile;

/// <summary>
/// Passfile entity.
/// </summary>
public interface IPassFile : IPassFileTimestamps
{
    /// <summary>
    /// Identifier of owner user.
    /// </summary>
    int UserId { get; }

    /// <summary>
    /// Content type.
    /// </summary>
    PassFileType Type { get; }

    /// <summary>
    /// Name.
    /// </summary>
    string Name { get; set; }

    /// <summary>
    /// Distinctive color (HEX).
    /// </summary>
    string? Color { get; set; }

    /// <summary>
    /// Content version.
    /// </summary>
    int Version { get; }

    /// <summary>
    /// Timestamp of creation.
    /// </summary>
    DateTime CreatedOn { get; }

    /// <summary>
    /// Assigned marks.
    /// </summary>
    PassFileMark Mark { get; set; }

    /// <summary>
    /// Origin passfile information from the server
    /// at the moment of the first unsynchronized change, if any.
    /// </summary>
    public IPassFileTimestamps? Origin { get; }
}

/// <summary>
/// Passfile entity with content.
/// </summary>
public interface IPassFile<TContent> : IPassFile
    where TContent : class
{
    /// <inheritdoc cref="IPassFileContent{TData}"/>
    IPassFileContent<TContent> Content { get; set; }
}