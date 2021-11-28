namespace PassMeta.DesktopApp.Ui.Views.Storage
{
    using Avalonia;
    using Avalonia.Controls;
    using Avalonia.Markup.Xaml;
    using Common.Models.Entities;
    using Utils.Extensions;
    using ViewModels.Storage;

    public class PassFileWindow : Window
    {
        public PassFile? PassFile { get; private set; }
        
        public PassFileWindow()
        {
            AvaloniaXamlLoader.Load(this);
            this.CorrectMainWindowFocusWhileOpened();
#if DEBUG
            this.AttachDevTools();
#endif
        }

        private new PassFileViewModel? DataContext
        {
            get => (PassFileViewModel?)base.DataContext;
            init => base.DataContext = value;
        }

        public PassFileWindow(PassFile passFile) : this()
        {
            PassFile = passFile.Id > 0 ? passFile : null;
            
            DataContext = new PassFileViewModel(passFile, Close);
            DataContext!.OnUpdate += actualPassFile =>
            {
                PassFile = actualPassFile?.Id > 0 ? actualPassFile : null;
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