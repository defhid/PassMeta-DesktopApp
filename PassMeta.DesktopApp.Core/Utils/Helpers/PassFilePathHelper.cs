using System;
using PassMeta.DesktopApp.Common.Constants;
using PassMeta.DesktopApp.Common.Enums;

namespace PassMeta.DesktopApp.Core.Utils.Helpers;

/// <summary>
/// Information about passfiles paths.
/// </summary>
public static class PassFilePathHelper
{
    /// <summary>
    /// Relative path to the file that contains passfiles list.
    /// </summary>
    public const string PassFileListName = "__all__";

    /// <summary>
    /// Get name for encrypted passfile content with extension.
    /// </summary>
    public static string GetPassFileContentName(PassFileType fileType, int fileId, int version)
        => $"{fileId}v{version}.{fileType.ToString().ToLower()}.{PassFileExternalFormat.Encrypted.Extension}";

    /// <summary>
    /// Get predicate for matching encrypted passfile content name with extension.
    /// </summary>
    public static Func<string, bool> GetPassFileContentNamePattern(int fileId, int version)
    {
        var start = $"{fileId}v{version}";
        var end = '.' + PassFileExternalFormat.Encrypted.Extension;

        return x => x.StartsWith(start) && x.EndsWith(end);
    }
    
    /// <summary>
    /// Get predicate for matching encrypted passfile content name with extension.
    /// </summary>
    public static Func<string, bool> GetPassFileContentNamePattern(int fileId)
    {
        var start = $"{fileId}v";
        var end = '.' + PassFileExternalFormat.Encrypted.Extension;

        return x => x.StartsWith(start) && x.EndsWith(end);
    }
    
    /// <summary>
    /// Get the version of encrypted passfile content by its name with extension.
    /// </summary>
    public static int? GetPassFileVersionFromName(string fileName)
    {
        var vIndex = fileName.IndexOf('v');
        var dotIndex = fileName.IndexOf('.');

        return vIndex > 0 &&
               dotIndex > vIndex &&
               int.TryParse(fileName.AsSpan(vIndex + 1, dotIndex - vIndex - 1), out var version)
            ? version
            : null;
    }
}