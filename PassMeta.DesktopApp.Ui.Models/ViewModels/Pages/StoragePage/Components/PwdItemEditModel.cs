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

    public event Action<PwdItemEditModel>? OnDelete;

    public event Action<PwdItemEditModel, int>? OnMove;

    public string? Usernames { get; set; }

    public string? Password
    {
        get => _password;
        set => this.RaiseAndSetIfChanged(ref _password, value);
    }

    public string? Remark { get; set; }

    public ReactCommand DeleteCommand { get; }

    public ReactCommand UpCommand { get; }

    public ReactCommand DownCommand { get; }

    public ReactCommand OpenPopupGenerator { get; }

    public PopupGeneratorModel PopupGenerator { get; }

    public IObservable<bool> PopupGeneratorCanBeOpened { get; }

    public bool IsEmpty => string.IsNullOrWhiteSpace(Usernames) && 
                           string.IsNullOrEmpty(Password) &&
                           string.IsNullOrWhiteSpace(Remark);

    public PwdItem ToItem() => new()
    {
        Usernames = NormalizeUsernames(Usernames?.Split('\n')),
        Password = Password ?? string.Empty,
        Remark = Remark?.Trim() ?? string.Empty
    };

    public static PwdItemEditModel From(PwdItem item) => new()
    {
        Usernames = string.Join('\n', NormalizeUsernames(item.Usernames)),
        Password = item.Password,
        Remark = item.Remark.Trim()
    };

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