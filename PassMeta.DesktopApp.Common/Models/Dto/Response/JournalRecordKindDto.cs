namespace PassMeta.DesktopApp.Common.Models.Dto.Response;

/// <summary>
/// Journal record kind DTO.
/// </summary>
public class JournalRecordKindDto
{
    /// <summary></summary>
    public JournalRecordKindDto()
    {
        Name ??= string.Empty;
    }

    /// <summary>
    /// Kind identifier.
    /// </summary>
    public int Id { get; init; }

    /// <summary>
    /// Kind name.
    /// </summary>
    public string Name { get; init; }

    /// <inheritdoc />
    public override string ToString() => Name;
}