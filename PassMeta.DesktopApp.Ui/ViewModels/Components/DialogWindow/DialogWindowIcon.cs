namespace PassMeta.DesktopApp.Ui.ViewModels.Components.DialogWindow
{
    public class DialogWindowIcon
    {
        public bool Visible { get; }
        
        public string? Source { get; }

        public static readonly DialogWindowIcon Hidden = new(false);
        public static readonly DialogWindowIcon Info = new(true, "Assets/DialogWindow/Info.png");
        public static readonly DialogWindowIcon Error = new(true, "Assets/DialogWindow/Error.png");
        public static readonly DialogWindowIcon Failure = new(true, "Assets/DialogWindow/Warning.png");
        public static readonly DialogWindowIcon Confirm = new(true, "Assets/DialogWindow/Confirm.png");
        public static readonly DialogWindowIcon Success = new(true, "Assets/DialogWindow/Success.png");

        private DialogWindowIcon(bool visible)
        {
            Visible = visible;
            Source = null;
        }

        private DialogWindowIcon(bool visible, string source)
        {
            Visible = visible;
            Source = source;
        }
    }
}