using System.Diagnostics;
using PassMeta.DesktopApp.Common.Abstractions.Utils.ValueMapping;

namespace PassMeta.DesktopApp.Common.Utils.ValueMapping;

/// <summary>
/// Mapping to constant string value.
/// </summary>
public class MapToString<TValueFrom> : IValueMapping<TValueFrom, string>
    where TValueFrom : notnull
{
    /// <inheritdoc />
    public TValueFrom From { get; }

    /// <inheritdoc />
    public string To { get; }

    /// <summary></summary>
    public MapToString(TValueFrom valueFrom, string valueTo)
    {
        Debug.Assert(valueFrom is not null);
        From = valueFrom;
        To = valueTo;
    }
}