namespace PassMeta.DesktopApp.Common.Utils.Mapping
{
    using Abstractions.Utils.Mapping;

    /// <summary>
    /// Mapping from one constant value to another.
    /// </summary>
    public class Map<TValueFrom, TValueTo> : IMapping<TValueFrom, TValueTo> 
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
}