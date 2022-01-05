namespace PassMeta.DesktopApp.Ui.Models.DialogWindow
{
    public class DialogWindowNumericBox : IDialogWindowInputBox
    {
        public bool Visible { get; }
        
        public string Placeholder { get; }
        
        public double Value { get; }

        public DialogWindowNumericBox(bool visible)
        {
            Visible = visible;
            Placeholder = "";
            Value = 0d;
        }

        public DialogWindowNumericBox(bool visible, string placeholder, double defaultValue)
        {
            Visible = visible;
            Placeholder = placeholder;
            Value = defaultValue;
        }
    }
}