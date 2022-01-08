namespace PassMeta.DesktopApp.Ui.ViewModels.Main.DialogWindow.Components
{
    public class NumericInputBox : IInputBox
    {
        public bool Visible { get; }
        
        public string Placeholder { get; }
        
        public double Value { get; }

        public NumericInputBox(bool visible)
        {
            Visible = visible;
            Placeholder = "";
            Value = 0d;
        }

        public NumericInputBox(bool visible, string placeholder, double defaultValue)
        {
            Visible = visible;
            Placeholder = placeholder;
            Value = defaultValue;
        }
    }
}