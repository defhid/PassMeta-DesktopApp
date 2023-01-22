using System;
using PassMeta.DesktopApp.Common.Enums;
using PassMeta.DesktopApp.Common.Models.Dto.Response;

namespace PassMeta.DesktopApp.Common.Abstractions.Entities.PassFile;

/// <summary>
/// Passfile entity.
/// </summary>
public interface IPassFile
{
    /// <summary>
    /// Identifier.
    /// </summary>
    int Id { get; }

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
    /// Timestamp of information change.
    /// </summary>
    DateTime InfoChangedOn { get; }

    /// <summary>
    /// Timestamp of data change.
    /// </summary>
    DateTime VersionChangedOn { get; }

    /// <summary>
    /// Timestamp of local deletion.
    /// </summary>
    DateTime? LocalDeletedOn { get; }

    /// <summary>
    /// Passfile information from the server at the current moment or
    /// the moment of the first unsynchronized change, if any.
    /// </summary>
    public PassFileInfoDto? RemoteOrigin { get; }
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