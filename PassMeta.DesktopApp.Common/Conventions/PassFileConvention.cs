using System;
using System.Collections.Generic;
using System.Linq;
using PassMeta.DesktopApp.Common.Enums;
using PassMeta.DesktopApp.Common.Models.Entities.PassFile;

namespace PassMeta.DesktopApp.Common.Conventions;

/// <summary>
/// Internal application rules for passfiles.
/// </summary>
public static class PassFileConvention
{
    private static readonly Dictionary<PassFileType, Type> EnumToEntity = new()
    {
        [PassFileType.Pwd] = typeof(PwdPassFile),
        [PassFileType.Txt] = typeof(TxtPassFile)
    };

    private static readonly Dictionary<Type, PassFileType> EntityToEnum = EnumToEntity.ToDictionary(
        x => x.Value,
        x => x.Key);

    /// <summary>
    /// Get passfile type by entity class.
    /// </summary>
    public static PassFileType GetPassFileType(Type entityType)
        => EntityToEnum.TryGetValue(entityType, out var enumValue)
            ? enumValue
            : throw new ArgumentOutOfRangeException(nameof(entityType), entityType, null);

    /// <summary>
    /// Get passfile type by entity class.
    /// </summary>
    public static PassFileType GetPassFileType<TPassFile>()
        where TPassFile : PassFile
        => GetPassFileType(typeof(TPassFile));
}