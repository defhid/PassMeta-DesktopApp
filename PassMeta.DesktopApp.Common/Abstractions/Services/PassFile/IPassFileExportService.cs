namespace PassMeta.DesktopApp.Common.Abstractions.Services.PassFile
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using PassMeta.DesktopApp.Common.Constants;
    using PassMeta.DesktopApp.Common.Models.Entities;

    /// <summary>
    /// Service for exporting pasfiles.
    /// </summary>
    public interface IPassFileExportService
    {
        /// <summary>
        /// Supported export formats.
        /// </summary>
        IEnumerable<ExternalFormat> SupportedFormats { get; }

        /// <summary>
        /// Export passfile to <paramref name="resultFilePath"/>.
        /// </summary>
        /// <returns>Success?</returns>
        Task<IResult> ExportAsync(PassFile passFile, string resultFilePath);
    }
}