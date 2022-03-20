namespace PassMeta.DesktopApp.Core.Services
{
    using System.Threading.Tasks;
    using Common.Interfaces;
    using Common.Interfaces.Services;
    using Common.Interfaces.Services.PassFile;
    using Common.Models;
    using Common.Models.Dto;
    using Common.Models.Entities;

    /// <inheritdoc />
    public class PassFileMergeService : IPassFileMergeService
    {
        private readonly IPassFileService _passFileService= EnvironmentContainer.Resolve<IPassFileService>();
        private readonly IDialogService _dialogService = EnvironmentContainer.Resolve<IDialogService>();

        /// <inheritdoc />
        public async Task<IResult<PassFileMerge>> LoadRemoteAndPrepareAsync(PassFile localPassFile)
        {
            var result = await _passFileService.GetPassFileRemoteAsync(localPassFile.Id);
            if (result.Bad)
                return Result.Failure<PassFileMerge>();

            var remotePassFile = result.Data!;
            
            throw new System.NotImplementedException();  // TODO
        }
    }
}