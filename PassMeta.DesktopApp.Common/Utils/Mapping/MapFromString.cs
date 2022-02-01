namespace PassMeta.DesktopApp.Common.Utils.Mapping
{
    using Interfaces.Mapping;

    /// <summary>
    /// Mapping from string to a constant value.
    /// </summary>
    public class MapFromString<TValueTo> : IMapping<string, TValueTo> 
        where TValueTo : notnull
    {
        /// <inheritdoc />
        public string From { get; }
        
        /// <inheritdoc />
        public TValueTo To { get; }
        
        /// <summary></summary>
        public MapFromString(string valueFrom, TValueTo valueTo)
        {
            From = valueFrom;
            To = valueTo;
        }
    }
}