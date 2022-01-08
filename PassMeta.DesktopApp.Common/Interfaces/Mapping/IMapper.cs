namespace PassMeta.DesktopApp.Common.Interfaces.Mapping
{
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    /// Simple mapper.
    /// </summary>
    public interface IMapper<TValueFrom, out TValueTo>
        where TValueFrom : notnull
        where TValueTo : notnull
    {
        /// <summary>
        /// Get mapped value corresponding to parameter <paramref name="value"/>
        /// </summary>
        [return: NotNullIfNotNull("value")]
        TValueTo? Map(TValueFrom? value);

        /// <summary>
        /// Get all mappings.
        /// </summary>
        IEnumerable<IMapping<TValueFrom, TValueTo>> GetMappings();
    }
}