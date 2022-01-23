namespace PassMeta.DesktopApp.Common.Interfaces.Mapping
{
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    /// Simple mapper.
    /// </summary>
    public interface IMapper<TValueFrom, TValueTo>
        where TValueFrom : notnull
        where TValueTo : notnull
    {
        /// <summary>
        /// Get mapped value corresponding to parameter <paramref name="value"/>
        /// </summary>
        [return: NotNullIfNotNull("value")]
        TValueTo? Map(TValueFrom? value);
        
        /// <summary>
        /// Get mapped value corresponding to parameter <paramref name="value"/>.
        /// If <paramref name="value"/> is null or wasn't found, return <paramref name="defaultValueTo"/>.
        /// </summary>
        [return: NotNullIfNotNull("defaultValueTo")]
        TValueTo? Map(TValueFrom? value, TValueTo? defaultValueTo);

        /// <summary>
        /// Get all mappings.
        /// </summary>
        IEnumerable<IMapping<TValueFrom, TValueTo>> GetMappings();
    }
}