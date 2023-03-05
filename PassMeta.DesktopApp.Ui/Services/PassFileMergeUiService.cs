using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;

using PassMeta.DesktopApp.Common;
using PassMeta.DesktopApp.Common.Abstractions;
using PassMeta.DesktopApp.Common.Abstractions.PassFileContext;
using PassMeta.DesktopApp.Common.Abstractions.Services;
using PassMeta.DesktopApp.Common.Abstractions.Services.PassFileServices;
using PassMeta.DesktopApp.Common.Abstractions.Utils.Logging;
using PassMeta.DesktopApp.Common.Enums;
using PassMeta.DesktopApp.Common.Extensions;
using PassMeta.DesktopApp.Common.Models;
using PassMeta.DesktopApp.Common.Models.Entities.PassFile;
using PassMeta.DesktopApp.Common.Models.Entities.PassFile.Data;
using PassMeta.DesktopApp.Ui.Models.Abstractions.Services;
using PassMeta.DesktopApp.Ui.Views.Storage;
using PassMeta.DesktopApp.Ui.Windows;

namespace PassMeta.DesktopApp.Ui.Services;

/// <inheritdoc />
public class PassFileMergeUiService : IPassFileMergeUiService
{
    private readonly IPwdPassFileMergePreparingService _mergePreparingService;
    private readonly IDialogService _dialogService;
    private readonly ILogsWriter _logger;

    public PassFileMergeUiService(IPwdPassFileMergePreparingService mergePreparingService, IDialogService dialogService, ILogsWriter logger)
    {
        _mergePreparingService = mergePreparingService;
        _dialogService = dialogService;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<IResult> LoadRemoteAndMergeAsync(PwdPassFile passFile, IPassFileContext<PwdPassFile> context, Window currentWindow)
    {
        try
        {
            var result = await ProcessPwdPassFile(passFile, currentWindow);
            if (result.Bad) return result;
                
            var updateResult = context.UpdateContent(passFile);
            if (!updateResult.Ok)
            {
                return Result.Failure();
            }

            passFile.Mark ^= PassFileMark.NeedsMerge;
            passFile.Mark |= PassFileMark.Merged;

            _dialogService.ShowInfo(Resources.PASSFILE__INFO_MERGED);
            return Result.Success();

        }
        catch (Exception ex)
        {
            _logger.Error(ex, nameof(PassFileMergeUiService));
            _dialogService.ShowError(ex.Message);
            return Result.Failure();
        }
    }

    private async Task<IResult> ProcessPwdPassFile(PwdPassFile passFile, Window currentWindow)
    {
        var mergeResult = await _mergePreparingService.LoadAndPrepareMergeAsync(passFile);
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

        passFile.Content = new PassFileContent<List<PwdSection>>(merge.Result, passFile.Content.PassPhrase!);
        return Result.Success();
    }
}