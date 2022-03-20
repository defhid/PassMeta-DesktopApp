namespace PassMeta.DesktopApp.Common.Interfaces.Services.PassFile
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Models;
    using Models.Entities;

    /// <summary>
    /// Service for importing passfiles.
    /// </summary>
    public interface IPassFileImportService
    {
        /// <summary>
        /// Import from file by its path (<paramref name="sourceFilePath"/>).
        /// </summary>
        /// <returns>
        /// Result with passfile data and passphrase to import.
        /// If failure, does not contain <see cref="ResultModel{TData}.Message"/>.
        /// </returns>
        Task<IResult<(List<PassFile.Section> Sections, string PassPhrase)>> ImportAsync(string sourceFilePath);
    }
}