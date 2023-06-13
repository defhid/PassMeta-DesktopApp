using System.Threading.Tasks;
using PassMeta.DesktopApp.Common.Abstractions.Utils.Logging;
using PassMeta.DesktopApp.Common.Extensions;
using PassMeta.DesktopApp.Common.Models.Entities.PassFile;
using PassMeta.DesktopApp.Ui.Models.Abstractions.Services;
using PassMeta.DesktopApp.Ui.Models.Providers;
using PassMeta.DesktopApp.Ui.Models.ViewModels.Windows.PassFileWin;
using PassMeta.DesktopApp.Ui.Views.Windows;

namespace PassMeta.DesktopApp.Ui.Services;

/// <inheritdoc />
public class PassFileOpenUiService<TPassFile> : IPassFileOpenUiService<TPassFile>
    where TPassFile : PassFile
{
    private readonly ILogsWriter _logger;

    public PassFileOpenUiService(ILogsWriter logger)
    {
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task ShowInfoAsync(TPassFile passFile, HostWindowProvider windowProvider)
    {
        var hostWindow = windowProvider.Window;
        if (hostWindow is null)
        {
            _logger.Error(GetType().Name + ": host window is currently null!");
            return;
        }

        var win = new PassFileWin { ViewModel = new PassFileWinModel<TPassFile>(passFile, windowProvider) };

        await win.ShowDialog(hostWindow);
    }
}