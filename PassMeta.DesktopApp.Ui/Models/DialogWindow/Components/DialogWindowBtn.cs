using PassMeta.DesktopApp.Common;
using PassMeta.DesktopApp.Common.Enums;

namespace PassMeta.DesktopApp.Ui.Models.DialogWindow.Components
{
    public class DialogWindowBtn
    {
        public bool Visible { get; }
        
        public string Text { get; }
        
        public DialogButton Type { get; }

        public static DialogWindowBtn Hidden => new(false);
        public static DialogWindowBtn Ok => new(true, Resources.DIALOG__BTN_OK, DialogButton.Ok);
        public static DialogWindowBtn Yes => new(true, Resources.DIALOG__BTN_YES, DialogButton.Yes);
        public static DialogWindowBtn No => new(true, Resources.DIALOG__BTN_NO, DialogButton.No);
        public static DialogWindowBtn Cancel => new(true, Resources.DIALOG__BTN_CANCEL, DialogButton.Cancel);
        public static DialogWindowBtn Close => new(true, Resources.DIALOG__BTN_CLOSE, DialogButton.Close);

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