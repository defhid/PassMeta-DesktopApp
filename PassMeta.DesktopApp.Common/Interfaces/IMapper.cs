namespace PassMeta.DesktopApp.Common.Interfaces
{
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    /// Simple mapper.
    /// </summary>
    public interface IMapper
    {
        /// <summary>
        /// Get mapped value corresponding to parameter <paramref name="value"/>
        /// </summary>
        [return: NotNullIfNotNull("value")]
        string? Map(string? value);
    }
}