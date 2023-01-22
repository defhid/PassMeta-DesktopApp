using System;
using PassMeta.DesktopApp.Common.Abstractions.Entities.PassFile;
using PassMeta.DesktopApp.Common.Enums;

namespace PassMeta.DesktopApp.Common.Models.Entities.PassFile;

/// <inheritdoc />
public abstract class PassFile : IPassFile
{
    /// <summary></summary>
    protected PassFile()
    {
        Name ??= "?";
    }

    /// <inheritdoc />
    public abstract PassFileType Type { get; }

    /// <inheritdoc />
    public int Id { get; init; }

    /// <inheritdoc />
    public int UserId { get; init; }

    /// <inheritdoc />
    public string Name { get; set; }

    /// <inheritdoc />
    public string? Color { get; set; }

    /// <inheritdoc />
    public int Version { get; set; }

    /// <inheritdoc />
    public DateTime CreatedOn { get; init; }

    /// <inheritdoc />
    public DateTime InfoChangedOn { get; init; }

    /// <inheritdoc />
    public DateTime VersionChangedOn { get; init; }

    /// <inheritdoc />
    public IPassFileTimestamps? Origin { get; init; }

    /// <inheritdoc />
    public override string ToString() 
        => $"<Passfile Id='{Id}', Name='{Name.Replace("'", "\"")}'>";
}

/// <inheritdoc cref="IPassFile{TContent}"/>
public abstract class PassFile<TContent> : PassFile, IPassFile<TContent>
    where TContent : class
{
    /// <summary></summary>
    protected PassFile(IPassFileContent<TContent> defaultContent)
    {
        Content = defaultContent;
    }

    /// <inheritdoc />
    public IPassFileContent<TContent> Content { get; set; }
}