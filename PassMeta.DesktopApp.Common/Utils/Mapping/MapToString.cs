namespace PassMeta.DesktopApp.Common.Utils.Mapping
{
    using System.Diagnostics;
    using Abstractions.Utils.Mapping;

    /// <summary>
    /// Mapping to constant string value.
    /// </summary>
    public class MapToString<TValueFrom> : IMapping<TValueFrom, string>
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
}