using System.Text.Json;
using PassMeta.DesktopApp.Common.Abstractions.Utils.PassFileContentSerializer;

namespace PassMeta.DesktopApp.Core.Utils.PassFileContentSerializer;

/// <inheritdoc />
public class PassFileContentSerializer<TContent> : IPassFileContentSerializer<TContent>
    where TContent : class, new()
{
    // ReSharper disable once StaticMemberInGenericType
    private static readonly JsonSerializerOptions SerializerPerformanceOptions = new()
    {
        WriteIndented = false
    };

    // ReSharper disable once StaticMemberInGenericType
    private static readonly JsonSerializerOptions SerializerUserFriendlyOptions = new()
    {
        WriteIndented = true
    };

    /// <inheritdoc />
    public byte[] Serialize(TContent? contentRaw, bool userFriendly)
        => JsonSerializer.SerializeToUtf8Bytes(contentRaw ?? new TContent(), userFriendly 
            ? SerializerUserFriendlyOptions
            : SerializerPerformanceOptions);

    /// <inheritdoc />
    public TContent Deserialize(byte[] contentBytes)
        => JsonSerializer.Deserialize<TContent>(contentBytes, SerializerPerformanceOptions) ?? new TContent();
}