using PassMeta.DesktopApp.Common.Abstractions.Utils.ValueMapping;

namespace PassMeta.DesktopApp.Common.Utils.ValueMapping;

/// <summary>
/// Mapping from one constant value to another.
/// </summary>
public class Map<TValueFrom, TValueTo> : IValueMapping<TValueFrom, TValueTo> 
    where TValueFrom : notnull 
    where TValueTo : notnull
{
    /// <inheritdoc />
    public TValueFrom From { get; }
        
    /// <inheritdoc />
    public TValueTo To { get; }
        
    /// <summary></summary>
    public Map(TValueFrom valueFrom, TValueTo valueTo)
    {
        From = valueFrom;
        To = valueTo;
    }
}