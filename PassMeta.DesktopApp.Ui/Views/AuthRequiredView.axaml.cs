using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using PassMeta.DesktopApp.Ui.Models;

namespace PassMeta.DesktopApp.Ui.Views;

public class AuthRequiredView : ReactiveUserControl<AuthRequiredModel>
{
    public AuthRequiredView()
    {
        AvaloniaXamlLoader.Load(this);
    }
}