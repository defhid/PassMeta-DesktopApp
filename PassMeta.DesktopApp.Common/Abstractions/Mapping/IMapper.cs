namespace PassMeta.DesktopApp.Common.Abstractions.Mapping
{
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using PassMeta.DesktopApp.Common.Utils.Mapping;

    /// <summary>
    /// Simple mapper.
    /// </summary>
    public interface IMapper<TValueFrom, TValueTo>
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
        IEnumerable<IMapping<TValueFrom, TValueTo>> GetMappings();
        
        /// <summary>
        /// Concatenate mappers.
        /// </summary>
        public static ICombinedMapper<TValueFrom, TValueTo> operator +(
            IMapper<TValueFrom, TValueTo> first, 
            IMapper<TValueFrom, TValueTo> second)
        {
            var combined = new DefaultCombinedMapper<TValueFrom, TValueTo>();
            combined.Mappers.Add(first);
            combined.Mappers.Add(second);
            return combined;
        }
    }
}