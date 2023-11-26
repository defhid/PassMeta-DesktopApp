using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using PassMeta.DesktopApp.Common.Abstractions.Utils.ValueMapping;

namespace PassMeta.DesktopApp.Common.Utils.ValueMapping;

/// <summary>
/// Default implementation of <see cref="IValuesMapper{TValueFrom,TValueTo}"/>.
/// </summary>
public sealed class ValuesMapper<TValueFrom, TValueTo> : IValuesMapper<TValueFrom, TValueTo>
    where TValueFrom : notnull
    where TValueTo : notnull
{
    /// <summary>
    /// All mappings.
    /// </summary>
    private readonly Dictionary<TValueFrom, IValueMapping<TValueFrom, TValueTo>> _mappings;

    /// <summary></summary>
    public ValuesMapper(IEnumerable<IValueMapping<TValueFrom, TValueTo>> mappings)
    {
        _mappings = mappings.ToDictionary(x => x.From, x => x);
    }

    /// <inheritdoc />
    public bool TryMap(TValueFrom? value, [NotNullWhen(true)] out TValueTo? valueTo)
    {
        if (value is null)
        {
            valueTo = default;
            return false;
        }

        if (_mappings.TryGetValue(value, out var mapping))
        {
            valueTo = mapping.To;
            return true;
        }

        valueTo = default;
        return false;
    }

    /// <inheritdoc />
    [return: NotNullIfNotNull("defaultValueTo")]
    public TValueTo? Map(TValueFrom? value, TValueTo? defaultValueTo)
    {
        return value is null
            ? defaultValueTo
            : _mappings.TryGetValue(value, out var mapping)
                ? mapping.To
                : defaultValueTo;
    }

    /// <inheritdoc />
    public bool Contains(TValueFrom valueFrom) => _mappings.ContainsKey(valueFrom);

    /// <inheritdoc />
    public IEnumerable<IValueMapping<TValueFrom, TValueTo>> GetMappings() => _mappings.Values;

    /// <summary>
    /// Create new <see cref="ValuesMapper{TValueFrom,TValueTo}"/> as combination
    /// of source <paramref name="valuesMapper"/> and additional <paramref name="mappings"/>.
    /// </summary>
    public static ValuesMapper<TValueFrom, TValueTo> operator +(
        ValuesMapper<TValueFrom, TValueTo> valuesMapper, 
        IEnumerable<IValueMapping<TValueFrom, TValueTo>> mappings) => new(valuesMapper._mappings.Values.Concat(mappings));
        
    /// <summary>
    /// Create <see cref="ValuesMapper{TValueFrom,TValueTo}"/> from <see cref="IValueMapping{TValueFrom,TValueTo}"/> array.
    /// </summary>
    public static implicit operator ValuesMapper<TValueFrom, TValueTo>(IValueMapping<TValueFrom, TValueTo>[] mappings) 
        => new(mappings);
}