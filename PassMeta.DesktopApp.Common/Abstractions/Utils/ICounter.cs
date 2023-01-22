using System.Threading.Tasks;

namespace PassMeta.DesktopApp.Common.Abstractions.Utils;

/// <summary>
/// Counter.
/// </summary>
/// <remarks>Stateless.</remarks>
public interface ICounter
{
    /// <summary>
    /// Get incremented value of the sequence with given name.
    /// </summary>
    Task<long> GetNextValueAsync(string name);
}