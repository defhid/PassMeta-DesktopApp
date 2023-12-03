using System.Collections.Generic;

namespace PassMeta.DesktopApp.Common.Models.Dto.Response;

/// <summary>
/// Result with list of <typeparamref name="TElement"/>.
/// </summary>
public class ListResult<TElement>
{
    /// <summary></summary>
    public ListResult()
    {
        List ??= new List<TElement>();
    }

    /// <summary>
    /// List of elements.
    /// </summary>
    public List<TElement> List { get; init; }
}