namespace PassMeta.DesktopApp.Core.Utils.Extensions;

using Common.Enums;
using Common.Models.Entities.Extra;

/// <summary>
/// Extension methods for <see cref="PassFileProblem"/> and <see cref="PassFileProblemKind"/>
/// </summary>
public static class PassFileProblemExtensions
{
    /// <summary>
    /// Create <see cref="PassFileProblem"/> with additional information.
    /// </summary>
    public static PassFileProblem ToProblemWithInfo(this PassFileProblemKind kind, string? info) 
        => new PassFileProblem(kind).WithInfo(info);
}