namespace PassMeta.DesktopApp.Ui.Views.Storage
{
    using Avalonia;
    using Avalonia.Controls;
    using Avalonia.Markup.Xaml;
    using Common.Models.Entities;
    using ViewModels.Storage;

    public class PassFileWindow : Window
    {
        public PassFile? PassFile { get; private set; }
        
        public PassFileWindow()
        {
            AvaloniaXamlLoader.Load(this);
#if DEBUG
            this.AttachDevTools();
#endif
        }
        
        public PassFileWindow(PassFile passFile) : this()
        {
            PassFile = passFile;
            DataContext = new PassFileViewModel(passFile, actualPassFile =>
            {
                PassFile = actualPassFile;
                Close();
            });
        }
    }
}