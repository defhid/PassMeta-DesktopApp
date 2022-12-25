using System;
using System.Threading.Tasks;

using PassMeta.DesktopApp.Common;
using PassMeta.DesktopApp.Common.Abstractions.Services;
using PassMeta.DesktopApp.Common.Abstractions.Services.Logging;
using PassMeta.DesktopApp.Core.Services.Extensions;

namespace PassMeta.DesktopApp.Ui.Services
{
    /// <inheritdoc />
    public class ClipboardService : IClipboardService
    {
        private readonly IDialogService _dialogService;
        private readonly ILogService _logger;

        /// <summary></summary>
        public ClipboardService(IDialogService dialogService, ILogService logger)
        {
            _dialogService = dialogService;
            _logger = logger;
        }

        /// <inheritdoc />
        public async Task<bool> TrySetTextAsync(string? text)
        {
            try
            {
                await TextCopy.ClipboardService.SetTextAsync(text ?? string.Empty);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Clipboard edit");
                _dialogService.ShowError(Resources.CLIPBOARD__UNKNOWN_ERR);
            }
            
            return true;
        }
    }
}