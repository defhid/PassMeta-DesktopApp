namespace PassMeta.DesktopApp.Common.Utils.Mapping
{
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using Interfaces.Mapping;

    /// <summary>
    /// Default implementation of <see cref="IMapper{TValueFrom,TValueTo}"/>.
    /// </summary>
    public sealed class SimpleMapper<TValueFrom, TValueTo> : IMapper<TValueFrom, TValueTo>
        where TValueFrom : notnull
        where TValueTo : notnull
    {
        /// <summary>
        /// All mappings.
        /// </summary>
        private readonly Dictionary<TValueFrom, IMapping<TValueFrom, TValueTo>> _mappings;

        /// <summary></summary>
        public SimpleMapper(IEnumerable<IMapping<TValueFrom, TValueTo>> mappings)
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
        public IEnumerable<IMapping<TValueFrom, TValueTo>> GetMappings() => _mappings.Values;

        /// <summary>
        /// Create new <see cref="SimpleMapper{TValueFrom,TValueTo}"/> as combination
        /// of source <paramref name="mapper"/> and additional <paramref name="mappings"/>.
        /// </summary>
        public static SimpleMapper<TValueFrom, TValueTo> operator +(
            SimpleMapper<TValueFrom, TValueTo> mapper, 
            IEnumerable<IMapping<TValueFrom, TValueTo>> mappings) => new(mapper._mappings.Values.Concat(mappings));
        
        /// <summary>
        /// Create <see cref="SimpleMapper{TValueFrom,TValueTo}"/> from <see cref="IMapping{TValueFrom,TValueTo}"/> array.
        /// </summary>
        public static implicit operator SimpleMapper<TValueFrom, TValueTo>(IMapping<TValueFrom, TValueTo>[] mappings) 
            => new(mappings);
    }
}