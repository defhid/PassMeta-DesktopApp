using System.Collections.Generic;
using System.Threading.Tasks;

namespace PassMeta.DesktopApp.Common.Abstractions.Services;

/// <summary>
/// Service for communication with app user to select file paths.
/// </summary>
/// <remarks>Singleton.</remarks>
public interface IFileDialogService
{
    /// <summary>
    /// Ask user a path to a single file for reading.
    /// </summary>
    Task<IResult<string>> AskForReadingAsync(IEnumerable<(string Name, List<string> Extensions)>? filters);
}