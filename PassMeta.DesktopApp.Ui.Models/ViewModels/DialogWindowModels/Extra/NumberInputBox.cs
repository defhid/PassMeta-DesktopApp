using System;

namespace PassMeta.DesktopApp.Ui.Models.DialogWindowModels;

public class NumberInputBox : IInputBox
{
    public Guid Tag { get; } = Guid.NewGuid();

    public bool Visible { get; }
        
    public string Placeholder { get; }
        
    public double Value { get; }

    public NumberInputBox(bool visible)
    {
        Visible = visible;
        Placeholder = "";
        Value = 0d;
    }

    public NumberInputBox(bool visible, string placeholder, double defaultValue)
    {
        Visible = visible;
        Placeholder = placeholder;
        Value = defaultValue;
    }
}