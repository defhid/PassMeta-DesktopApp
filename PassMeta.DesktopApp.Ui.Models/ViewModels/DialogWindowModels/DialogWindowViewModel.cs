using System;
using System.Collections.Generic;
using System.Linq;
using PassMeta.DesktopApp.Common.Enums;
using PassMeta.DesktopApp.Common.Extensions;
using ReactiveUI;

namespace PassMeta.DesktopApp.Ui.Models.DialogWindowModels;

public class DialogWindowViewModel : ReactiveObject
{
    public string Title { get; }

    public DialogWindowIcon WindowIcon { get; }
    public string Text { get; }
    public string? Details { get; }
    public bool DetailsVisible => !string.IsNullOrWhiteSpace(Details);

    public TextInputBox TextInputBox { get; }
    public NumberInputBox NumberInputBox { get; }
    public IInputBox? InputBoxFocused { get; }

    public IReadOnlyList<ResultButton> Buttons { get; }
    public ResultButton? ButtonPrimary { get; }

    public DialogButton? Result { get; set; }

    /// <summary>DEV constructor.</summary>
    public DialogWindowViewModel()
    {
        Title = "Dialog Title";
        WindowIcon = DialogWindowIcon.Hidden;
        Text = "Dialog Text";
        TextInputBox = new TextInputBox(false);
        NumberInputBox = new NumberInputBox(false);
        Buttons = new[]
        {
            new ResultButton(DialogButton.Yes),
            new ResultButton(DialogButton.No),
            new ResultButton(DialogButton.Cancel),
        };
    }

    /// <summary></summary>
    public DialogWindowViewModel(
        string title,
        string text,
        string? details,
        DialogButton[]? buttons,
        DialogWindowIcon? icon,
        IInputBox? input)
    {
        Title = title;
        WindowIcon = icon ?? DialogWindowIcon.Hidden;
        Text = text.Capitalize();
        Details = details.Capitalize();

        TextInputBox = input as TextInputBox ?? new TextInputBox(false);
        NumberInputBox = input as NumberInputBox ?? new NumberInputBox(false);

        InputBoxFocused = TextInputBox.Visible
            ? TextInputBox
            : NumberInputBox.Visible
                ? NumberInputBox
                : null;

        Buttons = (buttons ?? Array.Empty<DialogButton>())
            .Select(x => new ResultButton(x))
            .ToList();

        ButtonPrimary = Buttons.Any()
            ? Buttons.MinBy(btn => (int)btn.ButtonKind)
            : null;
    }
}