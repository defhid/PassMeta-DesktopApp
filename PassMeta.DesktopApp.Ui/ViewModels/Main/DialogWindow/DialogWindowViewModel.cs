namespace PassMeta.DesktopApp.Ui.ViewModels.Main.DialogWindow
{
    using System;
    using System.Linq;
    using Common.Enums;
    using Common.Utils.Extensions;
    using Components;
    using Constants;
    using ReactiveUI;

    public class DialogWindowViewModel : ReactiveObject
    {
        public string Title { get; }
        
        public DialogWindowIcon WindowIcon { get; }
        public string Text { get; }
        public string? Details { get; }
        public bool DetailsVisible => Details is not null;
        
        public TextInputBox WindowTextInputBox { get; }
        public NumericInputBox WindowNumericInputBox { get; }
        
        public ResultButton BtnOk { get; }
        public ResultButton BtnYes { get; }
        public ResultButton BtnNo { get; }
        public ResultButton BtnCancel { get; }
        public ResultButton BtnClose { get; }
        
        public DialogButton BtnFocused { get; }

        public DialogWindowViewModel()
        {
            Title = "";
            WindowIcon = DialogWindowIcon.Hidden;
            Text = "";
            WindowTextInputBox = new TextInputBox(false);
            WindowNumericInputBox = new NumericInputBox(false);
            BtnOk = new ResultButton(DialogButton.Ok, null);
            BtnYes = new ResultButton(DialogButton.Yes, null);
            BtnNo = new ResultButton(DialogButton.No, null);
            BtnCancel = new ResultButton(DialogButton.Cancel, null);
            BtnClose = new ResultButton(DialogButton.Close, null);
            BtnFocused = DialogButton.Ok;
        }
        
        public DialogWindowViewModel(string title,
            string text,
            string? details,
            DialogButton[] buttons,
            DialogWindowIcon? icon,
            IInputBox? input)
        {
            Title = title;
            WindowIcon = icon ?? DialogWindowIcon.Hidden;
            Text = text.Capitalize();
            Details = details.Capitalize();
            
            WindowTextInputBox = input is TextInputBox textBox
                ? textBox
                : new TextInputBox(false);
            WindowNumericInputBox = input is NumericInputBox numericBox
                ? numericBox
                : new NumericInputBox(false);

            if (!buttons.Any())
                throw new ArgumentException($"{nameof(DialogWindowViewModel)} buttons argument is empty!");

            BtnOk = new ResultButton(DialogButton.Ok, buttons);
            BtnYes = new ResultButton(DialogButton.Yes, buttons);
            BtnNo =  new ResultButton(DialogButton.No, buttons);
            BtnCancel =  new ResultButton(DialogButton.Cancel, buttons);
            BtnClose =  new ResultButton(DialogButton.Close, buttons);
            BtnFocused = (DialogButton)buttons.Min(btn => (int)btn);
        }
    }
}