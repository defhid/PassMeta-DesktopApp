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
    using Common.Constants;
    using Common.Models;
    using Common.Models.Entities;
    using Core;
    using Interfaces.UiServices;

    /// <inheritdoc />
    public class PassFileExportUiService : IPassFileExportUiService
    {
        private readonly ILogService _logger = EnvironmentContainer.Resolve<ILogService>();
        private readonly IDialogService _dialogService = EnvironmentContainer.Resolve<IDialogService>();
        
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
                InitialFileName = passFile.Name + ExternalFormat.PwdPassfileEncrypted.FullExtension,
                DefaultExtension = ExternalFormat.PwdPassfileEncrypted.FullExtension,
                Filters = exportService.SupportedFormats.Select(format => new FileDialogFilter
                {
                    Name = format.Name,
                    Extensions = { format.PureExtension }
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