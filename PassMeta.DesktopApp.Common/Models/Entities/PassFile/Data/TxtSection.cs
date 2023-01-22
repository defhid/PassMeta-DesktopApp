using System;
using Newtonsoft.Json;

namespace PassMeta.DesktopApp.Common.Models.Entities.PassFile.Data;

/// <summary>
/// Text section.
/// </summary>
public class TxtSection
{
    /// <summary>
    /// Section identifier.
    /// </summary>
    [JsonProperty("id")]
    public Guid Id { get; init; }

    /// <summary>
    /// Section name.
    /// </summary>
    [JsonProperty("nm")]
    public string Name { get; set; }

    /// <summary>
    /// Section content.
    /// </summary>
    [JsonProperty("it")]
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