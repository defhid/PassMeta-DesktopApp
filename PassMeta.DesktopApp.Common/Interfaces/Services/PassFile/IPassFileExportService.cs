namespace PassMeta.DesktopApp.Common.Interfaces.Services.PassFile
{
    using System.Threading.Tasks;
    using Models;
    using Models.Entities;

    /// <summary>
    /// Service for exporting pasfiles.
    /// </summary>
    public interface IPassFileExportService
    {
        /// <summary>
        /// Export passfile to <paramref name="resultFilePath"/>.
        /// </summary>
        /// <returns>Success?</returns>
        Task<IResult> ExportAsync(PassFile passFile, string resultFilePath);
    }
}