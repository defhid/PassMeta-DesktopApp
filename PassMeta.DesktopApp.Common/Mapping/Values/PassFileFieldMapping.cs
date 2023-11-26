using PassMeta.DesktopApp.Common.Abstractions.Utils.ValueMapping;
using PassMeta.DesktopApp.Common.Models.Entities.PassFile;
using PassMeta.DesktopApp.Common.Utils.ValueMapping;

namespace PassMeta.DesktopApp.Common.Mapping.Values;

/// <summary>
/// Mappers for fields of <see cref="PassFile"/>.
/// </summary>
public static class PassFileFieldMapping
{
    private static readonly ValuesMapper<string, string> FieldToNameMapper = new MapToResource<string>[]
    {
        new("passfile", () => Resources.DICT__PASSFILE),
        new("passfile_id", () => Resources.DICT__PASSFILE__ID),
        new("name", () => Resources.DICT__PASSFILE__NAME),
        new("color", () => Resources.DICT__PASSFILE__COLOR),
        new("created_on", () => Resources.DICT__PASSFILE__CREATED_ON)
    };

    /// <summary>
    /// Mapper for <see cref="PassFile"/> field names.
    /// </summary>
    public static IValuesMapper<string, string> FieldToName => FieldToNameMapper;
}