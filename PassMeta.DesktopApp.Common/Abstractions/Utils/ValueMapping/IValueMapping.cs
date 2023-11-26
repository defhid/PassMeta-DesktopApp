namespace PassMeta.DesktopApp.Common.Abstractions.Utils.ValueMapping;

/// <summary>
/// Mapping object.
/// </summary>
public interface IValueMapping<out TValueFrom, out TValueTo>
    where TValueFrom : notnull
    where TValueTo : notnull
{
    /// <summary>
    /// Mapping value from.
    /// </summary>
    TValueFrom From { get; }

    /// <summary>
    /// Mapping value to.
    /// </summary>
    TValueTo To { get; }
}