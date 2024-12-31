using System;
using System.Linq;
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
    public string[] Usernames { get; init; }

    /// <summary>
    /// One password.
    /// </summary>
    [JsonPropertyName("pwd")]
    public string Password { get; init; }

    /// <summary>
    /// Some comment.
    /// </summary>
    [JsonPropertyName("rmk")]
    public string Remark { get; init; }

    /// <summary></summary>
    public PwdItem()
    {
        Usernames ??= [];
        Password ??= string.Empty;
        Remark ??= string.Empty;
    }

    /// <summary>
    /// Memberwise clone.
    /// </summary>
    public PwdItem Copy() => (PwdItem)MemberwiseClone();

    /// <inheritdoc cref="Equals(object)" />
    public bool Equals(PwdItem? other)
        => !ReferenceEquals(null, other) &&
           Usernames.SequenceEqual(other.Usernames) &&
           Password == other.Password &&
           Remark == other.Remark;

    /// <inheritdoc />
    public override bool Equals(object? obj)
        => !ReferenceEquals(null, obj) &&
           (ReferenceEquals(this, obj) ||
            obj.GetType() == GetType() && Equals((PwdItem)obj));

    /// <inheritdoc />
    public override int GetHashCode() => HashCode.Combine(Usernames, Password, Remark);

    /// <summary></summary>
    public static bool operator ==(PwdItem? first, PwdItem? second) => first?.Equals(second) ?? ReferenceEquals(null, second);

    /// <summary></summary>
    public static bool operator !=(PwdItem? first, PwdItem? second) => !(first == second);
}