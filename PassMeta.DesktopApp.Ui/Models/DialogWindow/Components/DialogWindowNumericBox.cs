namespace PassMeta.DesktopApp.Ui.Models.DialogWindow.Components
{
    public class DialogWindowNumericBox : IDialogWindowInputBox
    {
        public bool Visible { get; }
        
        public string Placeholder { get; }
        
        public double DefaultValue { get; }

        public DialogWindowNumericBox(bool visible)
        {
            Visible = visible;
            Placeholder = "";
            DefaultValue = 0d;
        }

        public DialogWindowNumericBox(bool visible, string placeholder, double defaultValue)
        {
            Visible = visible;
            Placeholder = placeholder;
            DefaultValue = defaultValue;
        }
    }
}