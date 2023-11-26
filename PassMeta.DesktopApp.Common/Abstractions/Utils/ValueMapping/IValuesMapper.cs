using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using PassMeta.DesktopApp.Common.Utils.ValueMapping;

namespace PassMeta.DesktopApp.Common.Abstractions.Utils.ValueMapping;

/// <summary>
/// Simple mapper.
/// </summary>
public interface IValuesMapper<TValueFrom, TValueTo>
    where TValueFrom : notnull
    where TValueTo : notnull
{
    /// <summary>
    /// Try get a value corresponding to parameter <paramref name="value"/>
    /// </summary>
    bool TryMap(TValueFrom? value, [NotNullWhen(true)] out TValueTo? valueTo);
        
    /// <summary>
    /// Get a value corresponding to parameter <paramref name="value"/>.
    /// If <paramref name="value"/> is null or wasn't found, return <paramref name="defaultValueTo"/>.
    /// </summary>
    [return: NotNullIfNotNull("defaultValueTo")]
    TValueTo? Map(TValueFrom? value, TValueTo? defaultValueTo);

    /// <summary>
    /// Does mapper contain value to map it?
    /// </summary>
    bool Contains(TValueFrom valueFrom);

    /// <summary>
    /// Get all mappings.
    /// </summary>
    IEnumerable<IValueMapping<TValueFrom, TValueTo>> GetMappings();
        
    /// <summary>
    /// Concatenate mappers.
    /// </summary>
    public static ICombinedValuesMapper<TValueFrom, TValueTo> operator +(
        IValuesMapper<TValueFrom, TValueTo> first, 
        IValuesMapper<TValueFrom, TValueTo> second)
    {
        var combined = new CombinedValuesMapper<TValueFrom, TValueTo>();
        combined.Mappers.Add(first);
        combined.Mappers.Add(second);
        return combined;
    }
}