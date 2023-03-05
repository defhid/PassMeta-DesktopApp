using System;
using PassMeta.DesktopApp.Common.Enums;
using PassMeta.DesktopApp.Common.Mapping.Values;

namespace PassMeta.DesktopApp.Ui.Models.DialogWindowModels;

public class ResultButton
{
    public Guid Tag { get; } = Guid.NewGuid();

    public DialogButton ButtonKind { get; }

    public string Content => DialogButtonMapping.ButtonToName.Map(ButtonKind, "?");

    public ResultButton(DialogButton buttonKind)
    {
        ButtonKind = buttonKind;
    }
}