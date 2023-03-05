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

namespace PassMeta.DesktopApp.Ui.Services;

/// <inheritdoc />
public class PassFileExportUiService : IPassFileExportUiService
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
    public async Task SelectAndExportAsync<TContent>(PassFile<TContent> passFile, Window currentWindow)
        where TContent : class, new()
    {
        try
        {
            await SelectAndExportInternalAsync(passFile, currentWindow);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, GetType().Name);
            _dialogService.ShowError(ex.Message);
        }
    }

    private async Task SelectAndExportInternalAsync<TContent>(
        PassFile<TContent> passFile,
        Window currentWindow)
        where TContent : class, new()
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