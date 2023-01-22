using System;
using Newtonsoft.Json;

namespace PassMeta.DesktopApp.Common.Models.Entities.PassFile.Data;

/// <summary>
/// Item from password section.
/// </summary>
public class PwdItem
{
    /// <summary>
    /// Username list: email, phone, etc.
    /// </summary>
    [JsonProperty("usr")]
    public string[] Usernames { get; set; }

    /// <summary>
    /// One password.
    /// </summary>
    [JsonProperty("pwd")]
    public string Password { get; set; }

    /// <summary>
    /// Some comment.
    /// </summary>
    [JsonProperty("rmk")]
    public string Remark { get; set; }

    /// <summary></summary>
    public PwdItem()
    {
        Usernames ??= Array.Empty<string>();
        Password ??= string.Empty;
        Remark ??= string.Empty;
    }

    /// <summary>
    /// Memberwise clone.
    /// </summary>
    public PwdItem Copy() => (PwdItem)MemberwiseClone();
}