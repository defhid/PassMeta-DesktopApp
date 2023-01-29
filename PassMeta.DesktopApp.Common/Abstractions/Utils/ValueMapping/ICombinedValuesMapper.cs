using System.Collections.Generic;

namespace PassMeta.DesktopApp.Common.Abstractions.Utils.ValueMapping;

/// <summary>
/// Mapper from multiple mappers.
/// </summary>
public interface ICombinedValuesMapper<TValueFrom, TValueTo> : IValuesMapper<TValueFrom, TValueTo>
    where TValueFrom : notnull
    where TValueTo : notnull
{
    /// <summary>
    /// Get collection of mappers that make up the current.
    /// </summary>
    public IList<IValuesMapper<TValueFrom, TValueTo>> Mappers { get; }
}