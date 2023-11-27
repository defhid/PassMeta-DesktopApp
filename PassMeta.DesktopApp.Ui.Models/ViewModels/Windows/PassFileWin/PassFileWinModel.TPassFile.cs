using System.Linq;
using System.Threading.Tasks;
using PassMeta.DesktopApp.Common;
using PassMeta.DesktopApp.Common.Abstractions.PassFileContext;
using PassMeta.DesktopApp.Common.Abstractions.Services;
using PassMeta.DesktopApp.Common.Abstractions.Utils.Helpers;
using PassMeta.DesktopApp.Common.Extensions;
using PassMeta.DesktopApp.Common.Models.Entities.PassFile;
using PassMeta.DesktopApp.Ui.Models.Abstractions.Providers;
using PassMeta.DesktopApp.Ui.Models.Abstractions.Services;
using PassMeta.DesktopApp.Ui.Models.Constants;
using Splat;

namespace PassMeta.DesktopApp.Ui.Models.ViewModels.Windows.PassFileWin;

public class PassFileWinModel<TPassFile> : PassFileWinModel
    where TPassFile : PassFile
{
    private readonly IDialogService _dialogService;
    private readonly IPassFileContext<TPassFile> _pfContext;
    private readonly IPassFileContentHelper<TPassFile> _pfContentHelper;
    private readonly IHostWindowProvider _hostWindowProvider;

    public PassFileWinModel(TPassFile passFile, IHostWindowProvider hostWindowProvider) : base(passFile)
    {
        _dialogService = Locator.Current.Resolve<IDialogService>();
        _pfContext = Locator.Current.Resolve<IPassFileContextProvider>().For<TPassFile>();
        _pfContentHelper = Locator.Current.Resolve<IPassFileContentHelper<TPassFile>>();
        _hostWindowProvider = hostWindowProvider;
    }

    private new TPassFile PassFile => (TPassFile) base.PassFile;

    protected override void Save()
    {
        if (string.IsNullOrWhiteSpace(Name))
        {
            _dialogService.ShowFailure(Resources.PASSFILE__VALIDATION__INCORRECT_NAME);
            return;
        }

        PassFile.Name = Name.Trim();
        PassFile.Color = PassFileColor.List[SelectedColorIndex].Hex;

        var result = _pfContext.UpdateInfo(PassFile);
        if (result.Ok)
        {
            ChangedSource.OnNext(PassFile);
        }
    }

    protected override async Task ChangePasswordAsync()
    {
        var result = await _pfContentHelper.ChangePassPhraseAsync(PassFile);
        if (result.Ok)
        {
            _dialogService.ShowInfo(Resources.PASSFILE__INFO_PASSPHRASE_CHANGED);
            ChangedSource.OnNext(PassFile);
        }
        else
        {
            _dialogService.ShowWarning(Resources.PASSFILE__INFO_PASSPHRASE_NOT_CHANGED);
        }
    }

    protected override async Task ExportAsync()
    {
        var exportService = Locator.Current.Resolve<IPassFileExportUiService<TPassFile>>();

        await exportService.SelectAndExportAsync(PassFile, _hostWindowProvider);
    }

    protected override async Task RestoreAsync()
    {
        var restoreService = Locator.Current.Resolve<IPassFileRestoreUiService<TPassFile>>();

        var result = await restoreService.SelectAndRestoreAsync(PassFile, _pfContext, _hostWindowProvider);
        if (result.Ok)
        {
            ChangedSource.OnNext(PassFile);
        }
    }

    protected override async Task MergeAsync()
    {
        var mergeService = Locator.Current.Resolve<IPassFileMergeUiService<TPassFile>>();

        var result = await mergeService.LoadRemoteAndMergeAsync(PassFile, _pfContext, _hostWindowProvider);
        if (result.Ok)
        {
            ChangedSource.OnNext(PassFile);
        }
    }

    protected override async Task DeleteAsync()
    {
        var result = _pfContext.Delete(PassFile);
        if (result.Bad)
        {
            return;
        }

        if (_pfContext.CurrentList.Contains(PassFile))
        {
            ChangedSource.OnNext(PassFile);
        }
        else
        {
            await QuitAsync();
        }
    }
}