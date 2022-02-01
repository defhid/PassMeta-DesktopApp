namespace PassMeta.DesktopApp.Ui.Services
{
    using System;
    using System.Threading.Tasks;
    using Common;
    using Common.Interfaces.Services;
    using Core;

    public class ClipboardService : IClipboardService
    {
        private readonly IDialogService _dialogService;
        private readonly ILogService _logger;
        
        public ClipboardService()
        {
            _dialogService = EnvironmentContainer.Resolve<IDialogService>();
            _logger = EnvironmentContainer.Resolve<ILogService>();
        }
        
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