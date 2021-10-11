namespace PassMeta.DesktopApp.Ui.Models.DialogWindow.Components
{
    public class DialogWindowTextBox : IDialogWindowInputBox
    {
        public bool Visible { get; }
        
        public string Placeholder { get; }

        public string? Value
        {
            get => _value; 
            set => _value = value == string.Empty ? null : value;
        }

        public char? PasswordChar { get; }

        private string? _value;

        public DialogWindowTextBox(bool visible)
        {
            Visible = visible;
            Placeholder = "";
            PasswordChar = null;
        }

        public DialogWindowTextBox(bool visible, string placeholder, string? defaultValue, char passwordChar)
        {
            Visible = visible;
            Placeholder = placeholder;
            Value = defaultValue;
            PasswordChar = passwordChar;
        }
    }
}