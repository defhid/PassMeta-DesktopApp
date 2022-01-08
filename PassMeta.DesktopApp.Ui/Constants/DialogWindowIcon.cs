namespace PassMeta.DesktopApp.Ui.Constants
{
    /// <summary>
    /// Icons for dialog windows.
    /// </summary>
    public class DialogWindowIcon
    {
        /// <summary>
        /// Path to image.
        /// </summary>
        public string? Source { get; }
        
        /// <summary>
        /// <see cref="Source"/> is not null?
        /// </summary>
        public bool Visible => Source is not null;
        
        private DialogWindowIcon(string? source)
        {
            Source = source;
        }

        public static readonly DialogWindowIcon Hidden = new(null);
        
        public static readonly DialogWindowIcon Info = new("Assets/DialogWindow/Info.png");
        
        public static readonly DialogWindowIcon Error = new("Assets/DialogWindow/Error.png");
        
        public static readonly DialogWindowIcon Failure = new("Assets/DialogWindow/Warning.png");
        
        public static readonly DialogWindowIcon Confirm = new("Assets/DialogWindow/Confirm.png");
        
        public static readonly DialogWindowIcon Success = new("Assets/DialogWindow/Success.png");
    }
}