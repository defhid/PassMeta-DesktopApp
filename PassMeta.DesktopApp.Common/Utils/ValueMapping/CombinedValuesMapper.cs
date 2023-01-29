using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using PassMeta.DesktopApp.Common.Abstractions.Utils.ValueMapping;

namespace PassMeta.DesktopApp.Common.Utils.ValueMapping;

/// <summary>
/// Default implemantation of <see cref="ICombinedValuesMapper{TValueFrom,TValueTo}"/>.
/// </summary>
internal sealed class CombinedValuesMapper<TValueFrom, TValueTo> : ICombinedValuesMapper<TValueFrom, TValueTo>
    where TValueFrom : notnull
    where TValueTo : notnull
{
    /// <inheritdoc />
    public IList<IValuesMapper<TValueFrom, TValueTo>> Mappers { get; } = new List<IValuesMapper<TValueFrom, TValueTo>>();

    /// <inheritdoc />
    public bool TryMap(TValueFrom? value, [NotNullWhen(true)] out TValueTo? valueTo)
    {
        if (value is null)
        {
            valueTo = default;
            return false;
        }
            
        foreach (var mapper in Mappers)
        {
            if (mapper.Contains(value))
            {
                valueTo = mapper.Map(value, default!);
                return true;
            }
        }

        valueTo = default;
        return false;
    }

    /// <inheritdoc />
    [return: NotNullIfNotNull("defaultValueTo")]
    public TValueTo? Map(TValueFrom? value, TValueTo? defaultValueTo)
    {
        if (value is null) return defaultValueTo;
            
        foreach (var mapper in Mappers)
        {
            if (mapper.Contains(value))
            {
                return mapper.Map(value, default!);
            }
        }

        return defaultValueTo;
    }

    /// <inheritdoc />
    public bool Contains(TValueFrom valueFrom) => Mappers.Any(m => m.Contains(valueFrom));

    /// <inheritdoc />
    public IEnumerable<IValueMapping<TValueFrom, TValueTo>> GetMappings() =>
        Mappers.Select(m => m.GetMappings()).SelectMany(m => m);
}