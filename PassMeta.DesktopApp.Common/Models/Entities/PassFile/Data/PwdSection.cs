using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;

namespace PassMeta.DesktopApp.Common.Models.Entities.PassFile.Data;

/// <summary>
/// Password section.
/// </summary>
public class PwdSection
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
    /// Website address.
    /// </summary>
    [JsonPropertyName("url")]
    public string WebsiteUrl { get; set; }

    /// <summary>
    /// Section items.
    /// </summary>
    [JsonPropertyName("it")]
    public List<PwdItem> Items { get; set; }

    /// <summary></summary>
    public PwdSection()
    {
        Id = Id == default ? Guid.NewGuid() : Id;
        Name ??= "?";
        WebsiteUrl ??= string.Empty;
        Items ??= new List<PwdItem>();
    }

    /// <summary>
    /// Deep copy of this section.
    /// </summary>
    public PwdSection Copy()
    {
        var copy = (PwdSection) MemberwiseClone();
        copy.Items = copy.Items.Select(i => i.Copy()).ToList();
        return copy;
    }
}