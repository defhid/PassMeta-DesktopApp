using System.Threading.Tasks;
using PassMeta.DesktopApp.Common.Abstractions.Utils.Logging;
using PassMeta.DesktopApp.Common.Models.Entities.PassFile;
using PassMeta.DesktopApp.Ui.Models.Abstractions.Providers;
using PassMeta.DesktopApp.Ui.Models.Abstractions.Services;

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
    public Task ShowInfoAsync(TPassFile passFile, IHostWindowProvider windowProvider)
    {
        var win = windowProvider.Window;

        return Task.CompletedTask;

        // var win = new PassFileWin<TPassFile> { ViewModel = new PassFileWinModel<TPassFile>(passFile, windowProvider) };
        //
        // await win.ShowDialog(hostWindow);
    }
}