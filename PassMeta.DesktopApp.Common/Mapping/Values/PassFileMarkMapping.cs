using PassMeta.DesktopApp.Common.Abstractions.Utils.ValueMapping;
using PassMeta.DesktopApp.Common.Enums;
using PassMeta.DesktopApp.Common.Utils.ValueMapping;

namespace PassMeta.DesktopApp.Common.Mapping.Values;

/// <summary>
/// Mappers for <see cref="PassFileMark"/>
/// </summary>
public static class PassFileMarkMapping
{
    private static readonly ValuesMapper<PassFileMark, string> MarkToNameMapper = new MapToResource<PassFileMark>[]
    {
        new(PassFileMark.NeedsMerge, () => Resources.PASSFILE_MARK__NEEDS_MERGE),
        new(PassFileMark.Merged, () => Resources.PASSFILE_MARK__NEEDS_MERGE),
        new(PassFileMark.DownloadingError, () => Resources.DICT__PASSFILE_MARK__DOWNLOAD_ERR),
        new(PassFileMark.UploadingError, () => Resources.PASSFILE_MARK__UPLOAD_ERR),
        new(PassFileMark.RemoteDeletingError, () => Resources.PASSFILE_MARK__REMOTE_DELETING_ERR),
        new(PassFileMark.OtherError, () => Resources.PASSFILE_MARK__OTHER_ERR),
    };

    /// <summary>
    /// Mapper for <see cref="PassFileMark"/> names.
    /// </summary>
    public static IValuesMapper<PassFileMark, string> MarkToName => MarkToNameMapper;
}