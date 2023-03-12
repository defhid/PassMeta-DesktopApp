using System;
using System.Collections.Generic;
using System.Linq;
using PassMeta.DesktopApp.Common.Enums;
using PassMeta.DesktopApp.Common.Extensions;
using ReactiveUI;

namespace PassMeta.DesktopApp.Ui.Models.ViewModels.Windows.DialogWin;

/// <summary>
/// Dialog window ViewModel.
/// </summary>
public class DialogWinModel : ReactiveObject
{
    /// <summary></summary>
    public string Title { get; }

    /// <summary></summary>
    public DialogWindowIcon WindowIcon { get; }
    
    /// <summary></summary>
    public string Text { get; }
    
    /// <summary></summary>
    public string? Details { get; }
    
    /// <summary></summary>
    public bool DetailsVisible => !string.IsNullOrWhiteSpace(Details);

    /// <summary></summary>
    public TextInputBox TextInputBox { get; }
    
    /// <summary></summary>
    public NumberInputBox NumberInputBox { get; }
    
    /// <summary></summary>
    public IInputBox? InputBoxFocused { get; }

    /// <summary></summary>
    public IReadOnlyList<ResultButton> Buttons { get; }
    
    /// <summary></summary>
    public ResultButton? ButtonPrimary { get; }

    /// <summary></summary>
    public DialogButton? Result { get; set; }

    /// <summary></summary>
    [Obsolete("PREVIEW constructor")]
    public DialogWinModel()
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
    public DialogWinModel(
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