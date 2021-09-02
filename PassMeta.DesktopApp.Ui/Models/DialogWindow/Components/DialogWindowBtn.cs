using PassMeta.DesktopApp.Common.Enums;
using PassMeta.DesktopApp.Core;

namespace PassMeta.DesktopApp.Ui.Models.DialogWindow.Components
{
    public class DialogWindowBtn
    {
        public bool Visible { get; }
        
        public string Text { get; }
        
        public DialogButton Type { get; }

        public static readonly DialogWindowBtn Hidden = new(false);
        public static readonly DialogWindowBtn Ok = new(true, Resources.DIALOG__BTN_OK, DialogButton.Ok);
        public static readonly DialogWindowBtn Yes = new(true, Resources.DIALOG__BTN_YES, DialogButton.Yes);
        public static readonly DialogWindowBtn No = new(true, Resources.DIALOG__BTN_NO, DialogButton.No);
        public static readonly DialogWindowBtn Cancel = new(true, Resources.DIALOG__BTN_CANCEL, DialogButton.Cancel);
        public static readonly DialogWindowBtn Close = new(true, Resources.DIALOG__BTN_CLOSE, DialogButton.Close);

        private DialogWindowBtn(bool visible)
        {
            Visible = visible;
            Text = "";
            Type = DialogButton.Hidden;
        }

        private DialogWindowBtn(bool visible, string text, DialogButton type)
        {
            Visible = visible;
            Text = text;
            Type = type;
        }
    }
}