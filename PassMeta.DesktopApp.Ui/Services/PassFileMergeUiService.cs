using System;
using System.Threading.Tasks;
using Avalonia.Controls;

using PassMeta.DesktopApp.Common;
using PassMeta.DesktopApp.Common.Abstractions;
using PassMeta.DesktopApp.Common.Abstractions.Services;
using PassMeta.DesktopApp.Common.Abstractions.Services.PassFileServices;
using PassMeta.DesktopApp.Common.Abstractions.Utils.Logging;
using PassMeta.DesktopApp.Common.Enums;
using PassMeta.DesktopApp.Common.Models;
using PassMeta.DesktopApp.Common.Models.Entities.PassFile;
using PassMeta.DesktopApp.Core;
using PassMeta.DesktopApp.Core.Utils;
using PassMeta.DesktopApp.Core.Services.Extensions;
using PassMeta.DesktopApp.Ui.Interfaces.Services;
using PassMeta.DesktopApp.Ui.Views.Storage;

namespace PassMeta.DesktopApp.Ui.Services;

/// <inheritdoc />
public class PassFileMergeUiService : IPassFileMergeUiService
{
    private readonly IDialogService _dialogService;
    private readonly ILogsWriter _logger;

    public PassFileMergeUiService(IDialogService dialogService, ILogsWriter logger)
    {
        _dialogService = dialogService;
        _logger = logger;
    }

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
        var mergeService = Locator.Current.Resolve<IPwdPassFileMergePreparingService>();
            
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