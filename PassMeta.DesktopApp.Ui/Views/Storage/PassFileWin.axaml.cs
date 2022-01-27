namespace PassMeta.DesktopApp.Ui.Views.Storage
{
    using Avalonia;
    using Avalonia.Controls;
    using Avalonia.Markup.Xaml;
    using Common.Models.Entities;
    using Utils.Extensions;
    using ViewModels.Storage.PassFileWin;

    public class PassFileWin : Window
    {
        public PassFile? PassFile { get; private set; }

        public bool PassFileChanged { get; private set; }

        private readonly bool _changeNameAdvice;
        
        private new PassFileWinViewModel? DataContext
        {
            get => (PassFileWinViewModel?)base.DataContext;
            init => base.DataContext = value;
        }
        
        public PassFileWin()
        {
            AvaloniaXamlLoader.Load(this);
            this.CorrectMainWindowFocusWhileOpened();
#if DEBUG
            this.AttachDevTools();
#endif
        }

        public PassFileWin(PassFile passFile) : this()
        {
            PassFile = passFile;
            DataContext = new PassFileWinViewModel(passFile.Copy());
            DataContext.ViewElements.Window = this;

            _changeNameAdvice = PassFile!.Name.Trim() == Common.Resources.PASSMANAGER__DEFAULT_NEW_PASSFILE_NAME;
        }

        public new void Close()
        {
            if (DataContext!.PassFileChanged)
            {
                PassFile = DataContext.PassFile;
                PassFileChanged = true;
            }

            base.Close();
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