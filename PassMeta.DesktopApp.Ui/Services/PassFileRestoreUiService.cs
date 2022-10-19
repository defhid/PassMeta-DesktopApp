namespace PassMeta.DesktopApp.Ui.Services
{
    using System;
    using System.IO;
    using System.Threading.Tasks;
    using Avalonia.Controls;
    using Common;
    using Common.Abstractions;
    using Common.Abstractions.Services;
    using Common.Abstractions.Services.PassFile;
    using Common.Models;
    using Common.Models.Entities;
    using Core;
    using Core.Utils;
    using Interfaces.UiServices;
    using Views.Storage;

    /// <inheritdoc />
    public class PassFileRestoreUiService : IPassFileRestoreUiService
    {
        private readonly ILogService _logger = EnvironmentContainer.Resolve<ILogService>();
        private readonly IDialogService _dialogService = EnvironmentContainer.Resolve<IDialogService>();
        
        /// <inheritdoc />
        public async Task<IResult> SelectAndRestoreAsync(PassFile passFile, Window currentWindow)
        {
            try
            {
                var selectResult = await new PassFileRestoreWin(passFile).ShowDialog<IResult?>(currentWindow);
                if (selectResult?.Ok is not true)
                {
                    return Result.Failure();
                }

                if (passFile.LocalDeleted)
                {
                    var restoreResult = PassFileManager.Restore(passFile);
                    if (restoreResult.Bad)
                        _dialogService.ShowError(restoreResult.Message!);

                    return restoreResult;
                }
                
                return await _RestoreAsync(passFile, selectResult);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, nameof(PassFileRestoreUiService));
                _dialogService.ShowError(ex.Message);
                return Result.Failure();
            }
        }

        private async Task<IResult> _RestoreAsync(PassFile passFile, IResult selectResult)
        {
            var pathResult = selectResult as IResult<string>;
            var pfResult = selectResult as IResult<PassFile>;

            if (pathResult is not null)
            {
                var importService = EnvironmentContainer.Resolve<IPassFileImportService>(passFile.Type.ToString());
                
                var importResult = await importService.ImportAsync(passFile, pathResult.Data!, passFile.PassPhrase);
                if (importResult.Bad)
                {
                    return importResult;
                }
            }
            else if (pfResult is not null)
            {
                passFile.DataEncrypted = pfResult.Data!.DataEncrypted;
                passFile.PassPhrase = null;
            }
            else return Result.Failure();

            var updateResult = PassFileManager.UpdateData(passFile, pfResult is not null);
            if (updateResult.Ok)
            {
                _dialogService.ShowInfo(pathResult is null 
                    ? string.Format(Resources.PASSFILE__SUCCESS_RESTORE_FROM_SERVER, passFile.Name)
                    : string.Format(Resources.PASSFILE__SUCCESS_RESTORE_FROM_FILE, passFile.Name, Path.GetFileName(pathResult.Data)));
            }
            else
            {
                _dialogService.ShowError(updateResult.Message!);
            }

            return updateResult;
        }
    }
}