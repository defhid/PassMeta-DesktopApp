using PassMeta.DesktopApp.Common.Abstractions.Utils.ValueMapping;
using PassMeta.DesktopApp.Common.Models.Entities;
using PassMeta.DesktopApp.Common.Utils.ValueMapping;

namespace PassMeta.DesktopApp.Common.Mapping.Values;

/// <summary>
/// Mappers for sections of <see cref="Log"/>.
/// </summary>
public static class LogSectionMapping
{
    private static readonly ValuesMapper<string, string> SectionToFullNameMapper = new MapToResource<string>[]
    {
        new(Log.Sections.Info, () => Resources.LOG_SECTION__INFO),
        new(Log.Sections.Warning, () => Resources.LOG_SECTION__WARN),
        new(Log.Sections.Error, () => Resources.LOG_SECTION__ERROR),
        new(Log.Sections.Unknown, () => Resources.LOG_SECTION__UNKNOWN),
    };

    private static readonly ValuesMapper<string, string> SectionToShortNameMapper = new MapToResource<string>[]
    {
        new(Log.Sections.Info, () => Resources.LOG_SECTION__INFO_SHORT),
        new(Log.Sections.Warning, () => Resources.LOG_SECTION__WARN_SHORT),
        new(Log.Sections.Error, () => Resources.LOG_SECTION__ERROR_SHORT),
        new(Log.Sections.Unknown, () => Resources.LOG_SECTION__UNKNOWN_SHORT),
    };

    /// <summary>
    /// Mapper for <see cref="Log"/> sections full names.
    /// </summary>
    public static IValuesMapper<string, string> SectionToFullName => SectionToFullNameMapper;

    /// <summary>
    /// Mapper for <see cref="Log"/> section short names.
    /// </summary>
    public static IValuesMapper<string, string> SectionToShortName => SectionToShortNameMapper;
}