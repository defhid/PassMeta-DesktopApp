namespace PassMeta.DesktopApp.Ui.Models.Constants
{
    using System;
    using Avalonia.Media;
    using Common;

    public class PassFileColor
    {
        /// <summary>
        /// Color in hex format without #, upper case.
        /// </summary>
        public readonly string? Hex;
        
        /// <summary>
        /// Color parsed in brush.
        /// </summary>
        public ISolidColorBrush? Brush { get; }
        
        /// <summary>
        /// PassMeta common color name.
        /// </summary>
        public string Name => _nameGetter();
        
        private readonly Func<string> _nameGetter;

        private PassFileColor(string? hex, Func<string> nameGetter)
        {
            Hex = hex?.TrimStart('#').ToUpper();
            Brush = hex is null ? null : new SolidColorBrush(Color.Parse(hex));
            _nameGetter = nameGetter;
        }

        public static readonly PassFileColor None = new(null, () => "-");
        public static readonly PassFileColor Red = new("#F11D1D", () => Resources.PASSFILE_COLOR__RED);
        public static readonly PassFileColor Blue = new("#1E90FF", () => Resources.PASSFILE_COLOR__BLUE);
        public static readonly PassFileColor Gray = new("#808080", () => Resources.PASSFILE_COLOR__GREY);
        public static readonly PassFileColor Pink = new("#FF69B4", () => Resources.PASSFILE_COLOR__PINK);
        public static readonly PassFileColor Teal = new("#02B5AB", () => Resources.PASSFILE_COLOR__TEAL);
        public static readonly PassFileColor Green = new("#32CD32", () => Resources.PASSFILE_COLOR__GREEN);
        public static readonly PassFileColor Orange = new("#FFA500", () => Resources.PASSFILE_COLOR__ORANGE);
        public static readonly PassFileColor Purple = new("#9370DB", () => Resources.PASSFILE_COLOR__PURPLE);
        public static readonly PassFileColor Yellow = new("#D5D500", () => Resources.PASSFILE_COLOR__YELLOW);
        
        public static PassFileColor[] List { get; } =
        {
            None,
            Red,
            Pink,
            Purple,
            Blue,
            Teal,
            Green,
            Yellow,
            Orange,
            Gray,
        };
    }
}