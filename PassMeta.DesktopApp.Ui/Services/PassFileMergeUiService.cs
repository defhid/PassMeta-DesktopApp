namespace PassMeta.DesktopApp.Ui.Services
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using Avalonia.Controls;
    using Common;
    using Common.Abstractions;
    using Common.Abstractions.Services;
    using Common.Abstractions.Services.PassFile;
    using Common.Enums;
    using Common.Models;
    using Common.Models.Entities;
    using Common.Models.Entities.Extra;
    using Core;
    using Core.Utils;
    using Interfaces.UiServices;
    using Views.Storage;

    /// <inheritdoc />
    public class PassFileMergeUiService : IPassFileMergeUiService
    {
        private readonly ILogService _logger = EnvironmentContainer.Resolve<ILogService>();
        private readonly IDialogService _dialogService = EnvironmentContainer.Resolve<IDialogService>();
        
        /// <inheritdoc />
        public async Task<IResult> LoadRemoteAndMergeAsync(PassFile passFile, Window currentWindow)
        {
            try
            {
                var result = await (passFile.Type switch
                {
                    PassFileType.Pwd => _ProcessPwdPassFile(passFile, currentWindow),
                    _ => throw new ArgumentOutOfRangeException(nameof(passFile.Type), passFile.Type, null)
                });

                if (result.Bad) return result;
                
                var updateResult = PassFileManager.UpdateData(passFile);
                if (updateResult.Ok)
                {
                    passFile.Problem = null;
                    PassFileManager.TryResetProblem(passFile.Id);

                    _dialogService.ShowInfo(Resources.PASSFILE__INFO_MERGED);
                    return Result.Success();
                }

                _dialogService.ShowError(updateResult.Message!);
                return Result.Failure();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, nameof(PassFileMergeUiService));
                _dialogService.ShowError(ex.Message);
                return Result.Failure();
            }
        }

        private static async Task<IResult> _ProcessPwdPassFile(PassFile passFile, Window currentWindow)
        {
            var mergeService = EnvironmentContainer.Resolve<IPwdMergePreparingService>();
            
            var mergeResult = await mergeService.LoadAndPrepareMergeAsync(passFile);
            if (mergeResult.Bad)
            {
                return Result.Failure();
            }

            var merge = mergeResult.Data!;
            if (merge.Conflicts.Any())
            {
                var result = await new PassFileMergeWin(merge).ShowDialog<IResult?>(currentWindow);
                if (result?.Ok is not true)
                {
                    return Result.Failure();
                }
            }

            passFile.PwdData = merge.ResultSections;
            passFile.Marks |= PassFileMark.Merged;
            return Result.Success();
        }
    }
}