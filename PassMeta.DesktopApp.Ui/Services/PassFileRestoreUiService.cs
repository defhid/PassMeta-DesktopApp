using System;
using System.IO;
using System.Threading.Tasks;
using Avalonia.Controls;
    
using PassMeta.DesktopApp.Common;
using PassMeta.DesktopApp.Common.Abstractions;
using PassMeta.DesktopApp.Common.Abstractions.Services;
using PassMeta.DesktopApp.Common.Abstractions.Services.PassFileServices;
using PassMeta.DesktopApp.Common.Abstractions.Utils;
using PassMeta.DesktopApp.Common.Abstractions.Utils.Logging;
using PassMeta.DesktopApp.Common.Extensions;
using PassMeta.DesktopApp.Common.Models;
using PassMeta.DesktopApp.Common.Models.Entities.PassFile;
using PassMeta.DesktopApp.Core;
using PassMeta.DesktopApp.Core.Services.Extensions;
    
using PassMeta.DesktopApp.Ui.Interfaces.UiServices;
using PassMeta.DesktopApp.Ui.Views.Storage;

namespace PassMeta.DesktopApp.Ui.Services;

/// <inheritdoc />
public class PassFileRestoreUiService : IPassFileRestoreUiService
{
    private readonly IDialogService _dialogService;
    private readonly ILogsWriter _logger;

    public PassFileRestoreUiService(IDialogService dialogService, ILogsWriter logger)
    {
        _dialogService = dialogService;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<IResult> SelectAndRestoreAsync<TPassFile, TContent>(
        IPassFileContext<TPassFile> pfContext,
        TPassFile passFile,
        Window currentWindow)
        where TPassFile : PassFile<TContent>
        where TContent : class, new()
    {
        try
        {
            var selectResult = await new PassFileRestoreWin(passFile).ShowDialog<IResult?>(currentWindow);
            if (selectResult?.Ok is not true)
            {
                return Result.Failure();
            }

            if (passFile.IsLocalDeleted())
            {
                return pfContext.Restore(passFile);
            }

            return await RestoreInternalAsync(passFile, selectResult);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, nameof(PassFileRestoreUiService));
            _dialogService.ShowError(ex.Message);
            return Result.Failure();
        }
    }

    private async Task<IResult> RestoreInternalAsync(PassFile passFile, IResult selectResult)
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
            passFile.ContentEncrypted = pfResult.Data!.ContentEncrypted;
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