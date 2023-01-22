using System;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
    
using PassMeta.DesktopApp.Common;
using PassMeta.DesktopApp.Common.Abstractions;
using PassMeta.DesktopApp.Common.Abstractions.Services;
using PassMeta.DesktopApp.Common.Abstractions.Services.Logging;
using PassMeta.DesktopApp.Common.Abstractions.Services.PassFile;
using PassMeta.DesktopApp.Common.Constants;
using PassMeta.DesktopApp.Common.Models;
using PassMeta.DesktopApp.Common.Models.Entities.PassFile;
using PassMeta.DesktopApp.Core;
using PassMeta.DesktopApp.Core.Services.Extensions;
using PassMeta.DesktopApp.Ui.Interfaces.UiServices;

namespace PassMeta.DesktopApp.Ui.Services
{
    /// <inheritdoc />
    public class PassFileExportUiService : IPassFileExportUiService
    {
        private readonly IDialogService _dialogService;
        private readonly ILogService _logger;

        public PassFileExportUiService(IDialogService dialogService, ILogService logger)
        {
            _dialogService = dialogService;
            _logger = logger;
        }

        /// <inheritdoc />
        public async Task<IResult> SelectAndExportAsync(PassFile passFile, Window currentWindow)
        {
            try
            {
                var exportService = EnvironmentContainer.Resolve<IPassFileExportService>(passFile.Type.ToString());

                return await _SelectAndExportAsync(passFile, exportService, currentWindow);
            }
            catch (Exception ex)
            {
                
                _logger.Error(ex, nameof(PassFileExportUiService));
                _dialogService.ShowError(ex.Message);
                return Result.Failure();
            }
        }

        private async Task<IResult> _SelectAndExportAsync(PassFile passFile, IPassFileExportService exportService, Window currentWindow)
        {
            var fileDialog = new SaveFileDialog
            {
                InitialFileName = passFile.Name + PassFileExternalFormat.PwdPassfileEncrypted.FullExtension,
                DefaultExtension = PassFileExternalFormat.PwdPassfileEncrypted.FullExtension,
                Filters = exportService.SupportedFormats.Select(format => new FileDialogFilter
                {
                    Name = format.Name,
                    Extensions = { format.Extension }
                }).ToList()
            };
            
            var filePath = await fileDialog.ShowAsync(currentWindow);
            if (string.IsNullOrEmpty(filePath))
            {
                return Result.Failure();
            }
            
            var result = await exportService.ExportAsync(passFile, filePath);
            if (result.Ok)
            {
                _dialogService.ShowInfo(string.Format(Resources.PASSFILE__SUCCESS_EXPORT, passFile.Name, filePath));
            }

            return result;
        }
    }
}