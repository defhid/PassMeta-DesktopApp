using System;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using PassMeta.DesktopApp.Common.Interfaces.Services;
using PassMeta.DesktopApp.Common.Models.Entities.Request;
using PassMeta.DesktopApp.Ui.ViewModels;
using PassMeta.DesktopApp.Ui.Views.Base;
using Splat;

namespace PassMeta.DesktopApp.Ui.Views
{
    // ReSharper disable once UnusedType.Global
    public class AuthView : ViewPage<AuthViewModel>
    {
        public AuthView()
        {
            AvaloniaXamlLoader.Load(this);
        }
        
        public override Task RefreshAsync()
        {
            throw new NotImplementedException();
        }

        private async void SignInBtn_OnClick(object? sender, RoutedEventArgs e)
        {
            await SignInAsync();
        }
        
        private async void SignUpBtn_OnClick(object? sender, RoutedEventArgs e)
        {
            var service = Locator.Current.GetService<IAuthService>()!;
            var result = await service.SignUpAsync(new SignUpPostData(
                DataContext!.Login ?? "", DataContext!.Password ?? "", "Unknown", "Unknown"));
            if (result.Ok)
                NavigateTo<AccountViewModel>();
        }

        private async void Input_OnKeyDown(object? sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter) await SignInAsync();
        }

        private void LoginTextBox__OnAttachedToVisualTree(object? sender, VisualTreeAttachmentEventArgs e)
        {
            (sender as TextBox)?.Focus();
        }
        
        private async Task SignInAsync()
        {
            var service = Locator.Current.GetService<IAuthService>()!;
            var result = await service.SignInAsync(new SignInPostData(DataContext!.Login ?? "", DataContext!.Password ?? ""));
            if (result.Ok)
                NavigateTo<AccountViewModel>();
        }
    }
}