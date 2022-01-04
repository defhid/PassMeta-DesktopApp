namespace PassMeta.DesktopApp.Common.Interfaces.Mapping
{
    /// <summary>
    /// Mapping object.
    /// </summary>
    public interface IMapping
    {
        /// <summary>
        /// Mapping value from.
        /// </summary>
        string From { get; }
        
        /// <summary>
        /// Mapping value to.
        /// </summary>
        string To { get; }
    }
}