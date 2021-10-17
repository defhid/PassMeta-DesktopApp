using System;
using System.Threading.Tasks;
using Avalonia.Markup.Xaml;
using PassMeta.DesktopApp.Ui.ViewModels;
using PassMeta.DesktopApp.Ui.Views.Base;

namespace PassMeta.DesktopApp.Ui.Views
{
    // ReSharper disable once UnusedType.Global
    public class AuthRequiredView : ViewPage<AuthRequiredViewModel>
    {
        public AuthRequiredView()
        {
            AvaloniaXamlLoader.Load(this);
        }
        
        public override Task RefreshAsync()
        {
            throw new NotImplementedException();
        }
    }
}