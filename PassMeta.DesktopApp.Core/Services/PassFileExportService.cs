namespace PassMeta.DesktopApp.Core.Services
{
    using System.Threading.Tasks;
    using Common.Interfaces.Services.PassFile;
    using Common.Models;
    using Common.Models.Entities;

    /// <inheritdoc />
    public class PassFileExportService : IPassFileExportService
    {
        /// <inheritdoc />
        public Task<Result> ExportAsync(PassFile passFile, string resultFilePath)
        {
            throw new System.NotImplementedException();
        }
    }
}