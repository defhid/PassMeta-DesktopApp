using System;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Platform.Storage;
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
    public async Task SelectAndExportAsync(TPassFile passFile, IStorageProvider storageProvider)
    {
        try
        {
            await SelectAndExportInternalAsync(passFile, storageProvider);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, GetType().Name);
            _dialogService.ShowError(ex.Message);
        }
    }

    private async Task SelectAndExportInternalAsync(TPassFile passFile, IStorageProvider storageProvider)
    {
        var file = await storageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
        {
            SuggestedFileName = passFile.Name + '.' + PassFileExternalFormat.Encrypted.Extension,
            DefaultExtension = '.' + PassFileExternalFormat.Encrypted.Extension,
            FileTypeChoices = _exportService.SupportedFormats.Select(format => new FilePickerFileType(format.Name)
            {
                Patterns = new[] { "*." + format.Extension }
            }).ToList(),
        });

        if (file is null)
        {
            return;
        }

        var result = await _exportService.ExportAsync(passFile, file.Path.AbsolutePath);
        if (result.Ok)
        {
            _dialogService.ShowInfo(string.Format(Resources.PASSFILE__SUCCESS_EXPORT, passFile.Name, file.Path.AbsolutePath));
        }
    }
}