namespace PassMeta.DesktopApp.Common.Utils.Mapping
{
    using Interfaces.Mapping;

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
            From = valueFrom;
            To = valueTo;
        }
    }
}