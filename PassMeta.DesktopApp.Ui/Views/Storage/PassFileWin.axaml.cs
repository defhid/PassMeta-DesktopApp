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
            DataContext = new PassFileWinViewModel(passFile.Copy(), Close);
        }

        private new void Close()
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
            if (PassFile!.LocalCreated is true)
                (sender as TextBox)?.Focus();
        }
        
        private void OkBtn__OnAttachedToVisualTree(object? sender, VisualTreeAttachmentEventArgs e)
        {
            if (PassFile!.LocalCreated is false)
                (sender as Button)?.Focus();
        }
    }
}