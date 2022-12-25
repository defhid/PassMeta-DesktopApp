namespace PassMeta.DesktopApp.Common.Abstractions.Utils.Mapping
{
    using System.Collections.Generic;

    /// <summary>
    /// Mapper from multiple mappers.
    /// </summary>
    public interface ICombinedMapper<TValueFrom, TValueTo> : IMapper<TValueFrom, TValueTo>
        where TValueFrom : notnull
        where TValueTo : notnull
    {
        /// <summary>
        /// Get collection of mappers that make up the current.
        /// </summary>
        public IList<IMapper<TValueFrom, TValueTo>> Mappers { get; }
    }
}