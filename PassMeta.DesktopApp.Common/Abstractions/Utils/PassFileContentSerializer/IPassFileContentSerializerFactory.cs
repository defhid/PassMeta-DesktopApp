namespace PassMeta.DesktopApp.Common.Abstractions.Utils.PassFileContentSerializer;

/// <summary>
/// <see cref="IPassFileContentSerializer{TContent}"/> factory.
/// </summary>
public interface IPassFileContentSerializerFactory
{
    /// <summary>
    /// Get <see cref="IPassFileContentSerializer{TContent}"/> by content type.
    /// </summary>
    IPassFileContentSerializer<TContent> For<TContent>()
        where TContent : class, new();
}