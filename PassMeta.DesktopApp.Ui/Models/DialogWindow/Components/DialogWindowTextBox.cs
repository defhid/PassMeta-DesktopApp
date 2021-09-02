namespace PassMeta.DesktopApp.Ui.Models.DialogWindow.Components
{
    public class DialogWindowTextBox : IDialogWindowInputBox
    {
        public bool Visible { get; }
        
        public string Placeholder { get; }
        
        public string DefaultValue { get; }

        public DialogWindowTextBox(bool visible)
        {
            Visible = visible;
            Placeholder = "";
            DefaultValue = "";
        }

        public DialogWindowTextBox(bool visible, string placeholder, string defaultValue)
        {
            Visible = visible;
            Placeholder = placeholder;
            DefaultValue = defaultValue;
        }
    }
}