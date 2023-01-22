using System.Linq;
using System.Runtime.CompilerServices;
using PassMeta.DesktopApp.Common.Models.Entities.PassFile.Data;

namespace PassMeta.DesktopApp.Core.Utils.Extensions;

/// <summary>
/// Extension methods for passfile contents.
/// </summary>
public static class PassFileContentExtensions
{
    /// <summary>
    /// Does <paramref name="left"/> section have any difference
    /// with <paramref name="right"/>?
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool DiffersFrom(this PwdSection left, PwdSection right)
        => left.Name != right.Name ||
           left.Items.Count != right.Items.Count ||
           left.Items.Any(lItem =>
               right.Items.All(rItem => rItem.DiffersFrom(lItem)));

    /// <summary>
    /// Does <paramref name="left"/> item have any difference
    /// with <paramref name="right"/>?
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool DiffersFrom(this PwdItem left, PwdItem right)
        => left.Password != right.Password || 
           left.Remark != right.Remark ||
           !left.Usernames.All(right.Usernames.Contains);
}