using PassMeta.DesktopApp.Common.Models.Entities.PassFile;

namespace PassMeta.DesktopApp.Ui.Views.Storage
{
    using Avalonia;
    using Avalonia.Controls;
    using Avalonia.Markup.Xaml;
    using Base;
    using Utils.Extensions;
    using ViewModels.Storage.PassFileRestoreWin;

    public class PassFileRestoreWin : WinView<PassFileRestoreWinViewModel>
    {
        public PassFileRestoreWin()
        {
            AvaloniaXamlLoader.Load(this);
            this.CorrectMainWindowFocusWhileOpened();
        }

        public PassFileRestoreWin(PassFile currentPassFile) : this()
        {
            DataContext = new PassFileRestoreWinViewModel(currentPassFile)
            {
                ViewElements = { Window = this }
            };

            Opened += (_, _) => DataContext.Load();
        }

        private void DataGrid_OnAttachedToVisualTree(object? sender, VisualTreeAttachmentEventArgs e)
        {
            DataContext!.ViewElements.DataGrid = (DataGrid)sender!;
        }
    }
}