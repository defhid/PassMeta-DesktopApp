using PassMeta.DesktopApp.Common.Abstractions.Utils.ValueMapping;
using PassMeta.DesktopApp.Common.Models.Entities;
using PassMeta.DesktopApp.Common.Utils.ValueMapping;

namespace PassMeta.DesktopApp.Common.Mapping.Values;

/// <summary>
/// Mappers for fields of <see cref="User"/>.
/// </summary>
public static class UserFieldMapping
{
    private static readonly ValuesMapper<string, string> FieldToNameMapper = new MapToResource<string>[]
    {
        new("user", () => Resources.DICT__USER),
        new("first_name", () => Resources.DICT__USER__FIRST_NAME),
        new("last_name", () => Resources.DICT__USER__LAST_NAME),
        new("login", () => Resources.DICT__USER__LOGIN),
        new("password", () => Resources.DICT__USER__PASSWORD),
        new("password_confirm", () => Resources.DICT__USER__PASSWORD_CONFIRM)
    };

    /// <summary>
    /// Mapper for <see cref="User"/> field names.
    /// </summary>
    public static IValuesMapper<string, string> FieldToName => FieldToNameMapper;
}