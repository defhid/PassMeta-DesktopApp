using System;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using PassMeta.DesktopApp.Common;
using PassMeta.DesktopApp.Common.Abstractions.Services;
using PassMeta.DesktopApp.Common.Abstractions.Services.PassFileServices;
using PassMeta.DesktopApp.Common.Abstractions.Utils.Logging;
using PassMeta.DesktopApp.Common.Constants;
using PassMeta.DesktopApp.Common.Extensions;
using PassMeta.DesktopApp.Common.Models.Entities.PassFile;
using PassMeta.DesktopApp.Ui.Models.Abstractions.Services;
using PassMeta.DesktopApp.Ui.Models.Providers;

namespace PassMeta.DesktopApp.Ui.Services;

/// <inheritdoc />
public class PassFileExportUiService<TPassFile, TContent> : IPassFileExportUiService<TPassFile>
    where TPassFile : PassFile<TContent>
    where TContent : class, new()
{
    private readonly IPassFileExportService _exportService;
    private readonly IDialogService _dialogService;
    private readonly ILogsWriter _logger;

    public PassFileExportUiService(
        IPassFileExportService exportService,
        IDialogService dialogService,
        ILogsWriter logger)
    {
        _exportService = exportService;
        _dialogService = dialogService;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task SelectAndExportAsync(TPassFile passFile, HostWindowProvider windowProvider)
    {
        var win = windowProvider.Window;
        if (win is null)
        {
            _logger.Error(GetType().Name + ": host window is currently null!");
            return;
        }

        try
        {
            await SelectAndExportInternalAsync(passFile, win);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, GetType().Name);
            _dialogService.ShowError(ex.Message);
        }
    }

    private async Task SelectAndExportInternalAsync(TPassFile passFile, Window currentWindow)
    {
        var fileDialog = new SaveFileDialog
        {
            InitialFileName = passFile.Name + '.' + PassFileExternalFormat.Encrypted.Extension,
            DefaultExtension = '.' + PassFileExternalFormat.Encrypted.Extension,
            Filters = _exportService.SupportedFormats.Select(format => new FileDialogFilter
            {
                Name = format.Name,
                Extensions = { format.Extension }
            }).ToList()
        };

        var filePath = await fileDialog.ShowAsync(currentWindow);
        if (string.IsNullOrEmpty(filePath))
        {
            return;
        }

        var result = await _exportService.ExportAsync(passFile, filePath);
        if (result.Ok)
        {
            _dialogService.ShowInfo(string.Format(Resources.PASSFILE__SUCCESS_EXPORT, passFile.Name, filePath));
        }
    }
}