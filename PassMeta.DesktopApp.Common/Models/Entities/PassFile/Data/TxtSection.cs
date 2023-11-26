using System;
using System.Text.Json.Serialization;

namespace PassMeta.DesktopApp.Common.Models.Entities.PassFile.Data;

/// <summary>
/// Text section.
/// </summary>
public class TxtSection
{
    /// <summary>
    /// Section identifier.
    /// </summary>
    [JsonPropertyName("id")]
    public Guid Id { get; init; }

    /// <summary>
    /// Section name.
    /// </summary>
    [JsonPropertyName("nm")]
    public string Name { get; set; }

    /// <summary>
    /// Section content.
    /// </summary>
    [JsonPropertyName("it")]
    public string Content { get; set; }

    /// <summary></summary>
    public TxtSection()
    {
        Id = Id == default ? Guid.NewGuid() : Id;
        Name ??= "?";
        Content ??= "";
    }

    /// <summary>
    /// Deep copy of this section.
    /// </summary>
    public TxtSection Copy() => (TxtSection)MemberwiseClone();
}