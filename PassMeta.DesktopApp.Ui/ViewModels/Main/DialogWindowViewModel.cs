using System;
using System.Linq;
using PassMeta.DesktopApp.Common.Enums;
using ReactiveUI;

namespace PassMeta.DesktopApp.Ui.ViewModels.Main
{
    using Components.DialogWindow;
    using Core.Utils.Extensions;
    using Enums;

    public class DialogWindowViewModel : ReactiveObject
    {
        public string Title { get; }
        
        public DialogWindowIcon WindowIcon { get; }
        
        public string Text { get; }
        
        public string? Details { get; }

        public bool DetailsVisible => Details is not null;
        
        public DialogWindowTextBox WindowTextBox { get; }
        
        public DialogWindowNumericBox WindowNumericBox { get; }
        
        public DialogWindowBtn WindowBtnOk { get; }
        
        public DialogWindowBtn WindowBtnYes { get; }
        
        public DialogWindowBtn WindowBtnNo { get; }
        
        public DialogWindowBtn WindowBtnCancel { get; }
        
        public DialogWindowBtn WindowBtnClose { get; }
        
        public string BtnFocusedTag { get; }

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
            BtnFocusedTag = ((int)DialogButton.Hidden).ToString();
        }
        
        public DialogWindowViewModel(string title,
            string text,
            string? details,
            DialogButton[] buttons,
            DialogWindowIcon? icon,
            IDialogWindowInputBox? input)
        {
            Title = title;
            WindowIcon = icon ?? DialogWindowIcon.Hidden;
            Text = text.Capitalize();
            Details = details.Capitalize();
            
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

            BtnFocusedTag = buttons.Min(btn => (int)btn).ToString();
        }
    }
}