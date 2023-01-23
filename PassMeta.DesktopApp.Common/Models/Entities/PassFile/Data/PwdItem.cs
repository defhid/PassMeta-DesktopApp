using System;
using System.Text.Json.Serialization;

namespace PassMeta.DesktopApp.Common.Models.Entities.PassFile.Data;

/// <summary>
/// Item from password section.
/// </summary>
public class PwdItem
{
    /// <summary>
    /// Username list: email, phone, etc.
    /// </summary>
    [JsonPropertyName("usr")]
    public string[] Usernames { get; set; }

    /// <summary>
    /// One password.
    /// </summary>
    [JsonPropertyName("pwd")]
    public string Password { get; set; }

    /// <summary>
    /// Some comment.
    /// </summary>
    [JsonPropertyName("rmk")]
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