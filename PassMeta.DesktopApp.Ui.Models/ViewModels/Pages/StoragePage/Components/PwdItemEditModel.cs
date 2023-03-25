using System;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using PassMeta.DesktopApp.Common.Models.Entities.PassFile.Data;
using ReactiveUI;
using ReactCommand = ReactiveUI.ReactiveCommand<System.Reactive.Unit, System.Reactive.Unit>;

namespace PassMeta.DesktopApp.Ui.Models.ViewModels.Pages.StoragePage.Components;

/// <summary>
/// <see cref="PwdItem"/> edit card ViewModel.
/// </summary>
public class PwdItemEditModel : ReactiveObject
{
    private readonly BehaviorSubject<bool> _isPopupGeneratorOpened = new(false);
    private string? _password;

    /// <summary></summary>
    public PwdItemEditModel()
    {
        DeleteCommand = ReactiveCommand.Create(() => OnDelete?.Invoke(this));
        UpCommand = ReactiveCommand.Create(() => OnMove?.Invoke(this, -1));
        DownCommand = ReactiveCommand.Create(() => OnMove?.Invoke(this, 1));

        OpenPopupGenerator = ReactiveCommand.Create(() =>
        {
            _isPopupGeneratorOpened.OnNext(false);
            _isPopupGeneratorOpened.OnNext(true);
        });

        PopupGenerator = new PopupGeneratorModel(_isPopupGeneratorOpened, pwd => Password = pwd);

        PopupGeneratorCanBeOpened = this.WhenAnyValue(btn => btn.Password)
            .Select(string.IsNullOrEmpty);
    }

    /// <summary></summary>
    public event Action<PwdItemEditModel>? OnDelete;

    /// <summary></summary>
    public event Action<PwdItemEditModel, int>? OnMove;

    /// <summary></summary>
    public string? Usernames { get; set; }

    /// <summary></summary>
    public string? Password
    {
        get => _password;
        set => this.RaiseAndSetIfChanged(ref _password, value);
    }

    /// <summary></summary>
    public string? Remark { get; set; }

    /// <summary></summary>
    public ReactCommand DeleteCommand { get; }

    /// <summary></summary>
    public ReactCommand UpCommand { get; }

    /// <summary></summary>
    public ReactCommand DownCommand { get; }

    /// <summary></summary>
    public ReactCommand OpenPopupGenerator { get; }

    /// <summary></summary>
    public PopupGeneratorModel PopupGenerator { get; }

    /// <summary></summary>
    public IObservable<bool> PopupGeneratorCanBeOpened { get; }

    /// <summary></summary>
    public bool IsEmpty => string.IsNullOrWhiteSpace(Usernames) && 
                           string.IsNullOrEmpty(Password) &&
                           string.IsNullOrWhiteSpace(Remark);

    /// <summary></summary>
    public PwdItem ToItem() => new()
    {
        Usernames = NormalizeUsernames(Usernames?.Split('\n')),
        Password = Password ?? string.Empty,
        Remark = Remark?.Trim() ?? string.Empty
    };

    /// <summary></summary>
    public static PwdItemEditModel From(PwdItem item) => new()
    {
        Usernames = string.Join('\n', NormalizeUsernames(item.Usernames)),
        Password = item.Password,
        Remark = item.Remark.Trim()
    };

    /// <summary></summary>
    public static PwdItemEditModel Empty() => new()
    {
        Usernames = string.Empty,
        Password = string.Empty,
        Remark = string.Empty
    };

    private static string[] NormalizeUsernames(string[]? usernames)
        => usernames is null
            ? Array.Empty<string>()
            : usernames.Select(x => x.Trim()).Where(x => x != string.Empty).ToArray();
}