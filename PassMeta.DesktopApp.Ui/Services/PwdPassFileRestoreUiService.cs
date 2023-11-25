using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using PassMeta.DesktopApp.Common;
using PassMeta.DesktopApp.Common.Abstractions;
using PassMeta.DesktopApp.Common.Abstractions.PassFileContext;
using PassMeta.DesktopApp.Common.Abstractions.Services;
using PassMeta.DesktopApp.Common.Abstractions.Services.PassFileServices;
using PassMeta.DesktopApp.Common.Abstractions.Utils.Logging;
using PassMeta.DesktopApp.Common.Extensions;
using PassMeta.DesktopApp.Common.Models;
using PassMeta.DesktopApp.Common.Models.Entities.PassFile;
using PassMeta.DesktopApp.Common.Models.Entities.PassFile.Data;
using PassMeta.DesktopApp.Ui.Models.Abstractions.Services;
using PassMeta.DesktopApp.Ui.Models.Providers;
using PassMeta.DesktopApp.Ui.Views.Windows;
using Splat;

namespace PassMeta.DesktopApp.Ui.Services;

/// <inheritdoc />
public class PwdPassFileRestoreUiService : IPassFileRestoreUiService<PwdPassFile>
{
    private readonly IDialogService _dialogService;
    private readonly ILogsWriter _logger;

    public PwdPassFileRestoreUiService(IDialogService dialogService, ILogsWriter logger)
    {
        _dialogService = dialogService;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<IResult> SelectAndRestoreAsync(
        PwdPassFile passFile,
        IPassFileContext<PwdPassFile> pfContext,
        HostWindowProvider windowProvider)
    {
        var win = windowProvider.Window;
        if (win is null)
        {
            _logger.Error(GetType().Name + ": host window is currently null!");
            return Result.Failure();
        }

        try
        {
            var selectResult = await new PassFileRestoreWin(passFile).ShowDialog<IResult?>(win);
            if (selectResult?.Ok is not true)
            {
                return Result.Failure();
            }

            if (passFile.IsLocalDeleted())
            {
                return pfContext.Restore(passFile);
            }

            return await RestoreInternalAsync(passFile, pfContext, selectResult);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, nameof(PwdPassFileRestoreUiService));
            _dialogService.ShowError(ex.Message);
            return Result.Failure();
        }
    }

    private async Task<IResult> RestoreInternalAsync(PwdPassFile passFile, IPassFileContext<PwdPassFile> pfContext, IResult selectResult)
    {
        var pathResult = selectResult as IResult<string>;

        if (pathResult is not null)
        {
            var importService = Locator.Current.Resolve<IPassFileImportService>();
                
            var importResult = await importService.ImportAsync(passFile, pathResult.Data!);
            if (importResult.Bad)
            {
                return importResult;
            }
        }
        else if (selectResult is IResult<PwdPassFile> pfResult)
        {
            passFile.Content = new PassFileContent<List<PwdSection>>(pfResult.Data!.Content.Encrypted!, passFile.Content.PassPhrase!);
        }
        else return Result.Failure();

        var updateResult = pfContext.UpdateContent(passFile);
        if (updateResult.Ok)
        {
            _dialogService.ShowInfo(pathResult is null 
                ? string.Format(Resources.PASSFILE__SUCCESS_RESTORE_FROM_SERVER, passFile.Name)
                : string.Format(Resources.PASSFILE__SUCCESS_RESTORE_FROM_FILE, passFile.Name, Path.GetFileName(pathResult.Data)));
        }

        return updateResult;
    }
}