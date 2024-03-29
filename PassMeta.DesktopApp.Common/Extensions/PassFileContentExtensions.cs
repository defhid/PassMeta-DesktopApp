using System.Linq;
using PassMeta.DesktopApp.Common.Models.Entities.PassFile.Data;

namespace PassMeta.DesktopApp.Common.Extensions;

/// <summary>
/// Extension methods for passfile contents.
/// </summary>
public static class PassFileContentExtensions
{
    /// <summary>
    /// Does <paramref name="left"/> section have any difference
    /// with <paramref name="right"/>?
    /// </summary>
    public static bool DiffersFrom(this PwdSection left, PwdSection right)
        => left.Name != right.Name ||
           left.WebsiteUrl != right.WebsiteUrl ||
           left.Items.Count != right.Items.Count ||
           !left.Items.SequenceEqual(right.Items);
}