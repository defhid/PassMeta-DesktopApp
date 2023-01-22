using System.Threading;
using System.Threading.Tasks;

namespace PassMeta.DesktopApp.Common.Abstractions.Utils;

/// <summary>
/// Counter.
/// </summary>
/// <remarks>Stateless.</remarks>
public interface ICounter
{
    /// <summary>
    /// Get incremented value of the sequence with given name
    /// that is greater than <paramref name="gt"/> parameter value.
    /// </summary>
    Task<long> GetNextValueAsync(string name, long gt = 0, CancellationToken cancellationToken = default);
}