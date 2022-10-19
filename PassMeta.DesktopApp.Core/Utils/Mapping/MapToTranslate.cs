namespace PassMeta.DesktopApp.Core.Utils.Mapping
{
    using System.Collections.Generic;
    using Common.Abstractions.Mapping;

    /// <summary>
    /// Mapping to values from dictionary-like object,
    /// where: keys - locale, values - specific text.
    /// </summary>
    public class MapToTranslate<TValueFrom> : IMapping<TValueFrom, string>
        where TValueFrom : notnull
    {
        private readonly IDictionary<string, string> _translates;

        /// <inheritdoc />
        public TValueFrom From { get; }

        /// <inheritdoc />
        public string To =>
            _translates.TryGetValue(AppConfig.Current.CultureCode, out var result)
                ? result
                : _translates.TryGetValue("default", out result)
                    ? result
                    : From.ToString() ?? string.Empty;

        /// <summary></summary>
        public MapToTranslate(TValueFrom valueFrom, IDictionary<string, string> translates)
        {
            From = valueFrom;
            _translates = translates;
        }
    }
}