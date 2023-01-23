using System.Collections.Generic;
using System.Threading.Tasks;
using PassMeta.DesktopApp.Common.Constants;
using PassMeta.DesktopApp.Common.Models.Entities.PassFile;

namespace PassMeta.DesktopApp.Common.Abstractions.Services.PassFileServices;

/// <summary>
/// Service for importing passfiles.
/// </summary>
public interface IPassFileImportService
{
    /// <summary>
    /// Supported export formats.
    /// </summary>
    IEnumerable<PassFileExternalFormat> SupportedFormats { get; }

    /// <summary>
    /// Import data to passfile from file by specified path (<paramref name="sourceFilePath"/>).
    /// </summary>
    Task<IResult> ImportAsync<TContent>(PassFile<TContent> toPassFile, string sourceFilePath, string? supposedPassPhrase = null)
        where TContent : class, new();
}