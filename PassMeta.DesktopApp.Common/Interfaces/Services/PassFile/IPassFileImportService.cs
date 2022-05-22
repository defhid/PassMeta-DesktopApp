namespace PassMeta.DesktopApp.Common.Interfaces.Services.PassFile
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Constants;
    using Models.Entities;

    /// <summary>
    /// Service for importing passfiles.
    /// </summary>
    public interface IPassFileImportService
    {
        /// <summary>
        /// Supported export formats.
        /// </summary>
        IEnumerable<ExternalFormat> SupportedFormats { get; }

        /// <summary>
        /// Import data to passfile from file by specified path (<paramref name="sourceFilePath"/>).
        /// </summary>
        Task<IResult> ImportAsync(PassFile toPassFile, string sourceFilePath, string? supposedPassPhrase = null);
    }
}