using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PassMeta.DesktopApp.Common.Abstractions.Utils.Mapping;

namespace PassMeta.DesktopApp.Common.Models.Dto.Response.OkBad;

/// <summary>
/// <see cref="OkBadResponse"/> additional information.
/// </summary>
public class OkBadMore
{
    /// <summary>
    /// Short reason.
    /// </summary>
    [JsonProperty("what")]
    public string? What { get; set; }
        
    /// <summary>
    /// Some text message.
    /// </summary>
    [JsonProperty("text")]
    public string? Text { get; init; }
        
    /// <summary>
    /// Json-information.
    /// </summary>
    [JsonProperty("info")]
    public JContainer? Info { get; init; }

    /// <summary>
    /// Sub-responses. Sub-error information list.
    /// </summary>
    [JsonProperty("sub")]
    public List<OkBadResponse>? Sub { get; init; }

    /// <summary>
    /// Build string with information from notnull fields.
    /// </summary>
    public override string ToString()
    {
        var builder = new List<string>();
            
        if (Text is not null)
            builder.Add(Text);
        if (Info is not null) 
            builder.Add($"{Resources.OKBAD_MORE__INFO}: {Info}");

        return string.Join(Environment.NewLine, builder);
    }
        
    /// <summary>
    /// Replace <see cref="What"/> field values with mapped values recursively.
    /// </summary>
    public void ApplyWhatMapping(IMapper<string, string> mapper)
    {
        What = mapper.Map(What, What);
        if (Sub is null) return;
        foreach (var sub in Sub)
        {
            sub.More?.ApplyWhatMapping(mapper);
        }
    }
}