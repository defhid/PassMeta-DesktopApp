namespace PassMeta.DesktopApp.Common.Interfaces.Services.PassFile
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Constants;
    using Models.Entities;

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