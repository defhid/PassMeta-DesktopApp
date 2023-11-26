using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PassMeta.DesktopApp.Common;
using PassMeta.DesktopApp.Common.Abstractions.App;
using PassMeta.DesktopApp.Common.Abstractions.Services;
using PassMeta.DesktopApp.Common.Extensions;
using PassMeta.DesktopApp.Common.Models.Entities.PassFile.Data;
using ReactiveUI;
using Splat;
using ReactCommand = ReactiveUI.ReactiveCommand<System.Reactive.Unit, System.Reactive.Unit>;

namespace PassMeta.DesktopApp.Ui.Models.ViewModels.Pages.StoragePage.Components;

/// <summary>
/// <see cref="PwdItem"/> view card ViewModel.
/// </summary>
public class PwdItemReadModel : ReactiveObject
{
    private readonly IDialogService _dialogService = Locator.Current.Resolve<IDialogService>();
    private readonly IClipboardService _clipboardService = Locator.Current.Resolve<IClipboardService>();
    private readonly IAppConfigProvider _appConfig = Locator.Current.Resolve<IAppConfigProvider>();
    private readonly List<string> _usernames;

    public PwdItemReadModel(PwdItem item)
    {
        _usernames = item.Usernames.Select(x => x.Trim()).Where(x => x != string.Empty).ToList();
        Password = item.Password;
        Remark = string.IsNullOrEmpty(item.Remark) ? null : $"#{item.Remark}";

        CopyUsernameCommand = ReactiveCommand.CreateFromTask(CopyUsernameAsync);
        CopyPasswordCommand = ReactiveCommand.CreateFromTask(CopyPasswordAsync);
    }

    public string Usernames => string.Join('\n', _usernames);
    
    public string Password { get; }
    
    public string? Remark { get; }

    public bool IsCommentTextVisible => Remark is not null;
    
    public char? PasswordChar => _appConfig.Current.HidePasswords ? '*' : null;

    public ReactCommand CopyUsernameCommand { get; }

    public ReactCommand CopyPasswordCommand { get; }

    private async Task CopyUsernameAsync()
    {
        var firstUsername = _usernames.FirstOrDefault() ?? string.Empty;

        if (await _clipboardService.TrySetTextAsync(firstUsername))
        {
            _dialogService.ShowInfo(string.Format(Resources.STORAGE__ITEM_INFO__USERNAME_COPIED, firstUsername));
        }
    }

    private async Task CopyPasswordAsync()
    {
        if (await _clipboardService.TrySetTextAsync(Password))
        {
            _dialogService.ShowInfo(Resources.STORAGE__ITEM_INFO__PASSWORD_COPIED);
        }
    }
}