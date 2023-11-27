using System;
using System.Threading.Tasks;
using PassMeta.DesktopApp.Common.Abstractions.Utils.Logging;
using PassMeta.DesktopApp.Common.Extensions;
using PassMeta.DesktopApp.Common.Models.Entities.PassFile;
using PassMeta.DesktopApp.Ui.Models.Abstractions.Providers;
using PassMeta.DesktopApp.Ui.Models.Abstractions.Services;
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
    public async Task ShowInfoAsync(TPassFile passFile, IHostWindowProvider windowProvider)
    {
        var hostWin = windowProvider.Window;

        try
        {
            var win = new PassFileWin { ViewModel = new PassFileWinModel<TPassFile>(passFile, windowProvider) };

            await win.ShowDialog(hostWin);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failed to open passfile window");
        }
    }
}