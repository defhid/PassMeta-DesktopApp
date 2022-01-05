namespace PassMeta.DesktopApp.Ui.Views.Storage
{
    using Avalonia;
    using Avalonia.Controls;
    using Avalonia.Markup.Xaml;
    using Common.Models.Entities;
    using Utils.Extensions;
    using PassFileWinViewModel = ViewModels.Storage.PassFileWin.PassFileWinViewModel;

    public class PassFileWin : Window
    {
        public PassFile? PassFile { get; private set; }
        
        public PassFileWin()
        {
            AvaloniaXamlLoader.Load(this);
            this.CorrectMainWindowFocusWhileOpened();
#if DEBUG
            this.AttachDevTools();
#endif
        }

        private new PassFileWinViewModel? DataContext
        {
            get => (PassFileWinViewModel?)base.DataContext;
            init => base.DataContext = value;
        }

        public PassFileWin(PassFile passFile) : this()
        {
            PassFile = passFile.Id > 0 ? passFile : null;
            
            DataContext = new PassFileWinViewModel(passFile, Close);
            DataContext!.OnUpdate += actualPassFile =>
            {
                PassFile = actualPassFile;
            };
        }
        
        private void NameTextBox__OnAttachedToVisualTree(object? sender, VisualTreeAttachmentEventArgs e)
        {
            if (PassFile?.Id is not >0)
                (sender as TextBox)?.Focus();
        }
        
        private void OkBtn__OnAttachedToVisualTree(object? sender, VisualTreeAttachmentEventArgs e)
        {
            if (PassFile?.Id > 0)
                (sender as Button)?.Focus();
        }
    }
}