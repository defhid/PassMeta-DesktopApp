namespace PassMeta.DesktopApp.Common.Abstractions.Utils.PassFileContentSerializer;

/// <summary>
/// Methods for serializing/deserializing passfile content.
/// </summary>
public interface IPassFileContentSerializer<TContent>
    where TContent : class
{
    /// <summary>
    /// Serialize passfile content to bytes.
    /// </summary>
    byte[] Serialize(TContent? contentRaw, bool userFriendly);

    /// <summary>
    /// Serialize passfile content to bytes.
    /// </summary>
    TContent Deserialize(byte[] contentBytes);
}