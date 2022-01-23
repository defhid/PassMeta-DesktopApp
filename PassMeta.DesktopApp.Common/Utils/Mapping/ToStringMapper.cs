namespace PassMeta.DesktopApp.Common.Utils.Mapping
{
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using Interfaces.Mapping;

    /// <summary>
    /// Simple value-to-string mapper.
    /// </summary>
    public class ToStringMapper<TValueFrom> : IMapper<TValueFrom, string>
        where TValueFrom : notnull
    {
        private readonly Dictionary<TValueFrom, IMapping<TValueFrom, string>> _mappings;

        /// <summary></summary>
        public ToStringMapper(IEnumerable<IMapping<TValueFrom, string>> mappings)
        {
            _mappings = mappings.ToDictionary(map => map.From, map => map);
        }

        /// <inheritdoc />
        [return: NotNullIfNotNull("value")]
        public string? Map(TValueFrom? value)
        {
            if (value is null) return null;
            
            return _mappings.TryGetValue(value, out var mapping) 
                ? mapping.To 
                : value.ToString() ?? string.Empty;
        }

        /// <inheritdoc />
        [return: NotNullIfNotNull("defaultValueTo")]
        public string? Map(TValueFrom? value, string? defaultValueTo)
        {
            if (value is null) return defaultValueTo;
            
            return _mappings.TryGetValue(value, out var mapping) 
                ? mapping.To 
                : defaultValueTo;
        }

        /// <inheritdoc />
        public IEnumerable<IMapping<TValueFrom, string>> GetMappings() => _mappings.Values;

        /// <summary>
        /// Create <see cref="ToStringMapper{TValueFrom}"/> from <see cref="MapToResource{TValueFrom}"/> array.
        /// </summary>
        public static implicit operator ToStringMapper<TValueFrom>(IMapping<TValueFrom, string>[] mappings) 
            => new(mappings);

        /// <summary>
        /// Create new <see cref="ToStringMapper{TValueFrom}"/> as combination
        /// of source <paramref name="toStringMapper"/> and additional <paramref name="mappings"/>.
        /// </summary>
        public static ToStringMapper<TValueFrom> operator +(ToStringMapper<TValueFrom> toStringMapper, IMapping<TValueFrom, string>[] mappings) 
            => new(toStringMapper._mappings.Values.Concat(mappings));
    }
}