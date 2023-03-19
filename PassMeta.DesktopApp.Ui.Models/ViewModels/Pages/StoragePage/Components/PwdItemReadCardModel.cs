using System;
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
public class PwdItemReadCardModel : ReactiveObject
{
    private readonly IDialogService _dialogService = Locator.Current.Resolve<IDialogService>();
    private readonly IClipboardService _clipboardService = Locator.Current.Resolve<IClipboardService>();
    private readonly IAppConfigProvider _appConfig = Locator.Current.Resolve<IAppConfigProvider>();
    private readonly List<string> _usernames;

    /// <summary></summary>
    public PwdItemReadCardModel(PwdItem item)
    {
        _usernames = item.Usernames.Select(x => x.Trim()).Where(x => x != string.Empty).ToList();
        Password = item.Password;
        Remark = item.Remark;

        CopyWhatCommand = ReactiveCommand.CreateFromTask(CopyWhatAsync);
        CopyPasswordCommand = ReactiveCommand.CreateFromTask(CopyPasswordAsync);
    }

    /// <summary></summary>
    [Obsolete("PREVIEW constructor")]
    public PwdItemReadCardModel() : this(new PwdItem
    {
        Usernames = new[] { "example_login" },
        Password = "example_pwd", 
        Remark = "example_remark"
    })
    {
    }

    /// <summary></summary>
    public string Usernames => string.Join('\n', _usernames);
    
    /// <summary></summary>
    public string Password { get; }
    
    /// <summary></summary>
    public string Remark { get; }

    /// <summary></summary>
    public bool IsCommentTextVisible => string.IsNullOrEmpty(Remark);
    
    /// <summary></summary>
    public char? PasswordChar => _appConfig.Current.HidePasswords ? '*' : null;

    /// <summary></summary>
    public ReactCommand CopyWhatCommand { get; }

    /// <summary></summary>
    public ReactCommand CopyPasswordCommand { get; }

    private async Task CopyWhatAsync()
    {
        var what = _usernames.FirstOrDefault() ?? string.Empty;

        if (await _clipboardService.TrySetTextAsync(what))
        {
            _dialogService.ShowInfo(string.Format(Resources.STORAGE__ITEM_INFO__WHAT_COPIED, what));
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