namespace PassMeta.DesktopApp.Common.Utils.Mapping
{
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using Interfaces.Mapping;

    /// <summary>
    /// Default implemantation of <see cref="ICombinedMapper{TValueFrom,TValueTo}"/>.
    /// </summary>
    internal sealed class DefaultCombinedMapper<TValueFrom, TValueTo> : ICombinedMapper<TValueFrom, TValueTo>
        where TValueFrom : notnull
        where TValueTo : notnull
    {
        /// <inheritdoc />
        public IList<IMapper<TValueFrom, TValueTo>> Mappers { get; } = new List<IMapper<TValueFrom, TValueTo>>();

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
        public IEnumerable<IMapping<TValueFrom, TValueTo>> GetMappings() =>
            Mappers.Select(m => m.GetMappings()).SelectMany(m => m);
    }
}