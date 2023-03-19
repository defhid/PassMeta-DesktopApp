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
public class PwdItemEditCardModel : ReactiveObject
{
    private readonly BehaviorSubject<bool> _isPopupGeneratorOpened = new(false);
    private string? _password;

    /// <summary></summary>
    public PwdItemEditCardModel(
        Action<PwdItemEditCardModel> onDelete,
        Action<PwdItemEditCardModel, int> onMove)
    {
        DeleteCommand = ReactiveCommand.Create(() => onDelete(this));
        UpCommand = ReactiveCommand.Create(() => onMove(this, -1));
        DownCommand = ReactiveCommand.Create(() => onMove(this, 1));

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
    public PwdItem ToItem() => new()
    {
        Usernames = NormalizeWhat().Split('\n'),
        Password = Password ?? string.Empty,
        Remark = Remark?.Trim() ?? string.Empty
    };

    private string NormalizeWhat()
        => string.IsNullOrWhiteSpace(Usernames)
            ? string.Empty
            : string.Join('\n', Usernames.Split('\n').Select(x => x.Trim()).Where(x => x != string.Empty));
}