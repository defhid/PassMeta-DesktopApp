using PassMeta.DesktopApp.Common.Utils.ValueMapping;

namespace PassMeta.DesktopApp.Ui.ViewModels.Main.DialogWindow.Components;

using System.Collections.Generic;
using System.Linq;
using Common;
using Common.Enums;

public class ResultButton
{
    public DialogButton ButtonKind { get; }
        
    public bool IsVisible { get; }

    public string? Content => IsVisible ? ButtonKindToName.Map(ButtonKind, "?") : null;
        
    public ResultButton(DialogButton buttonKind, IEnumerable<DialogButton>? requiredButtonKinds)
    {
        ButtonKind = buttonKind;
        IsVisible = requiredButtonKinds?.Contains(buttonKind) ?? false;
    }
        
    private static readonly ValuesMapper<DialogButton, string> ButtonKindToName = new MapToResource<DialogButton>[]
    {
        new(DialogButton.Ok, () => Resources.DIALOG__BTN_OK),
        new(DialogButton.Yes, () => Resources.DIALOG__BTN_YES),
        new(DialogButton.No, () => Resources.DIALOG__BTN_NO),
        new(DialogButton.Cancel, () => Resources.DIALOG__BTN_CANCEL),
        new(DialogButton.Close, () => Resources.DIALOG__BTN_CLOSE),
    };
}