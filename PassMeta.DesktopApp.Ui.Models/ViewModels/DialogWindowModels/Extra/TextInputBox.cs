using System;

namespace PassMeta.DesktopApp.Ui.Models.DialogWindowModels;

public class TextInputBox : IInputBox
{
    public Guid Tag { get; } = Guid.NewGuid(); 

    public bool Visible { get; }
        
    public string Placeholder { get; }
        
    private string? _value;
    public string? Value
    {
        get => _value; 
        set => _value = value == string.Empty ? null : value;
    }

    public char? PasswordChar { get; }
    public bool IsForPassword => PasswordChar is not null;

    public int SelectionStart { get; }
    public int SelectionEnd { get; }

    public TextInputBox(bool visible)
    {
        Visible = visible;
        Placeholder = "";
        PasswordChar = null;
    }

    public TextInputBox(bool visible, string placeholder, string? defaultValue, char? passwordChar, bool selectDefaultValue = true)
    {
        Visible = visible;
        Placeholder = placeholder;
        Value = defaultValue;
        PasswordChar = passwordChar;
        SelectionStart = selectDefaultValue ? 0 : (defaultValue ?? string.Empty).Length;
        SelectionEnd = (defaultValue ?? string.Empty).Length;
    }
}