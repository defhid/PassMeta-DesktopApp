using PassMeta.DesktopApp.Common.Abstractions.Utils.PassFileContentSerializer;

namespace PassMeta.DesktopApp.Core.Utils.PassFileContentSerializer;

/// <inheritdoc />
public class PassFileContentSerializerFactory : IPassFileContentSerializerFactory
{
    /// <inheritdoc />
    public IPassFileContentSerializer<TContent> For<TContent>()
        where TContent : class, new()
        => new PassFileContentSerializer<TContent>();
}