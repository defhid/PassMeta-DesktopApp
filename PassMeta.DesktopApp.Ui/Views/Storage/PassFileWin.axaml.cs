namespace PassMeta.DesktopApp.Ui.Views.Storage
{
    using Avalonia;
    using Avalonia.Controls;
    using Avalonia.Markup.Xaml;
    using Common.Models.Entities;
    using Utils.Extensions;
    using ViewModels.Base;
    using ViewModels.Storage.PassFileWin;

    public class PassFileWin : WinView<PassFileWinViewModel>
    {
        public PassFile? PassFile { get; private set; }

        public bool PassFileChanged { get; private set; }

        private readonly bool _changeNameAdvice;

        public PassFileWin()
        {
            AvaloniaXamlLoader.Load(this);
            this.CorrectMainWindowFocusWhileOpened();
        }

        public PassFileWin(PassFile passFile) : this()
        {
            PassFile = passFile;
            DataContext = new PassFileWinViewModel(passFile.Copy());
            DataContext.ViewElements.Window = this;

            _changeNameAdvice = PassFile!.Name.Trim() == Common.Resources.PASSMANAGER__DEFAULT_NEW_PASSFILE_NAME;
            Closing += (_, _) =>
            {
                if (DataContext!.PassFileChanged)
                {
                    PassFile = DataContext.PassFile;
                    PassFileChanged = true;
                }
            };
        }

        private void NameTextBox__OnAttachedToVisualTree(object? sender, VisualTreeAttachmentEventArgs e)
        {
            var textBox = (sender as TextBox)!;

            textBox.CaretIndex = textBox.Text.Length;

            if (_changeNameAdvice)
            {
                textBox.SelectionStart = 0;
                textBox.SelectionEnd = textBox.Text.Length;
                textBox.Focus();
            }
        }
        
        private void OkBtn__OnAttachedToVisualTree(object? sender, VisualTreeAttachmentEventArgs e)
        {
            if (!_changeNameAdvice) (sender as Button)!.Focus();
        }
    }
}