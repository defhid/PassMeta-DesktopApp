using System.Collections.Generic;
using System.Threading.Tasks;
using PassMeta.DesktopApp.Common.Constants;
using PassMeta.DesktopApp.Common.Models.Entities.PassFile;

namespace PassMeta.DesktopApp.Common.Abstractions.Services.PassFileServices;

/// <summary>
/// Service for exporting pasfiles.
/// </summary>
/// <remarks>Transient.</remarks>
public interface IPassFileExportService
{
    /// <summary>
    /// Supported export formats.
    /// </summary>
    IEnumerable<PassFileExternalFormat> SupportedFormats { get; }

    /// <summary>
    /// Export passfile to <paramref name="resultFilePath"/>.
    /// </summary>
    /// <returns>Success?</returns>
    Task<IResult> ExportAsync<TContent>(PassFile<TContent> passFile, string resultFilePath)
        where TContent : class, new();
}