using System;
using System.Linq;
using PassMeta.DesktopApp.Common.Enums;
using PassMeta.DesktopApp.Ui.Models.DialogWindow.Components;

namespace PassMeta.DesktopApp.Ui.ViewModels
{
    public class DialogWindowViewModel : ViewModelBase
    {
        public string Title { get; }
        
        public DialogWindowIcon WindowIcon { get; }
        
        public string Text { get; }
        
        public DialogWindowTextBox WindowTextBox { get; }
        
        public DialogWindowNumericBox WindowNumericBox { get; }
        
        public DialogWindowBtn WindowBtnOk { get; }
        
        public DialogWindowBtn WindowBtnYes { get; }
        
        public DialogWindowBtn WindowBtnNo { get; }
        
        public DialogWindowBtn WindowBtnCancel { get; }
        
        public DialogWindowBtn WindowBtnClose { get; }

        public DialogWindowViewModel()
        {
            Title = "";
            WindowIcon = DialogWindowIcon.Hidden;
            Text = "";
            WindowTextBox = new DialogWindowTextBox(false);
            WindowNumericBox = new DialogWindowNumericBox(false);
            WindowBtnOk = DialogWindowBtn.Hidden;
            WindowBtnYes = DialogWindowBtn.Hidden;
            WindowBtnNo = DialogWindowBtn.Hidden;
            WindowBtnCancel = DialogWindowBtn.Hidden;
            WindowBtnClose = DialogWindowBtn.Hidden;
        }
        
        public DialogWindowViewModel(string title,
            string text,
            DialogButton[] buttons,
            DialogWindowIcon? icon,
            IDialogWindowInputBox? input)
        {
            Title = title;
            WindowIcon = icon ?? DialogWindowIcon.Hidden;
            Text = text;
            
            WindowTextBox = input is DialogWindowTextBox textBox
                ? textBox
                : new DialogWindowTextBox(false);
            WindowNumericBox = input is DialogWindowNumericBox numericBox
                ? numericBox
                : new DialogWindowNumericBox(false);

            if (!buttons.Any())
                throw new ArgumentException($"{nameof(DialogWindowViewModel)} buttons argument is empty!");
            
            WindowBtnOk = buttons.Any(b => b == DialogButton.Ok)
                ? DialogWindowBtn.Ok
                : DialogWindowBtn.Hidden;
            WindowBtnYes = buttons.Any(b => b == DialogButton.Yes)
                ? DialogWindowBtn.Yes
                : DialogWindowBtn.Hidden;
            WindowBtnNo = buttons.Any(b => b == DialogButton.No)
                ? DialogWindowBtn.No
                : DialogWindowBtn.Hidden;
            WindowBtnCancel = buttons.Any(b => b == DialogButton.Cancel)
                ? DialogWindowBtn.Cancel
                : DialogWindowBtn.Hidden;
            WindowBtnClose = buttons.Any(b => b == DialogButton.Close)
                ? DialogWindowBtn.Close
                : DialogWindowBtn.Hidden;
        }
    }
}