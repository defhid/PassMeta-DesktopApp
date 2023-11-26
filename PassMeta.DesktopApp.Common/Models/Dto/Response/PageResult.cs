using System.Collections.Generic;

namespace PassMeta.DesktopApp.Common.Models.Dto.Response;

/// <summary>
/// Page result with list of <typeparamref name="TElement"/>.
/// </summary>
public class PageResult<TElement>
{
    /// <summary></summary>
    public PageResult()
    {
        List ??= new List<TElement>();
    }

    /// <summary>
    /// Page number, starting from 1.
    /// </summary>
    public int PageIndex { get; init; }

    /// <summary>
    /// Page size.
    /// </summary>
    public int PageSize { get; init; }

    /// <summary>
    /// Total found.
    /// </summary>
    public int Total { get; init; }

    /// <summary>
    /// List of elements.
    /// </summary>
    public List<TElement> List { get; init; }
}