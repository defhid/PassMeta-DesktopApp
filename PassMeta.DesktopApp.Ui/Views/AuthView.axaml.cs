using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using PassMeta.DesktopApp.Common.Interfaces.Services;
using PassMeta.DesktopApp.Common.Models;
using PassMeta.DesktopApp.Common.Models.Entities.Request;
using PassMeta.DesktopApp.Ui.ViewModels;
using Splat;

namespace PassMeta.DesktopApp.Ui.Views
{
    public class AuthView : ReactiveUserControl<AuthViewModel>
    {
        public AuthView()
        {
            AvaloniaXamlLoader.Load(this);
        }

        private async void SignInBtn_OnClick(object? sender, RoutedEventArgs e)
        {
            var result = ValidateAndGetData<SignInPostData>();
            if (!result.Ok) return;
            
            await Locator.Current.GetService<IDialogService>()
                .ShowInfoAsync("Здесь должен быть вход в систему...");
        }
        
        private async void SignUpBtn_OnClick(object? sender, RoutedEventArgs e)
        {
            var result = ValidateAndGetData<SignUpPostData>();
            if (!result.Ok) return;
            
            await Locator.Current.GetService<IDialogService>()
                .ShowInfoAsync("Здесь должна быть регистрация в системе...");
        }

        private Result<TData> ValidateAndGetData<TData>()
            where TData : SignInPostData
        {
            var context = (AuthViewModel)DataContext!;
            SignInPostData data;
            
            if (typeof(TData) == typeof(SignInPostData))
            {
                data = new SignInPostData
                {
                    
                };
            }
            else
            {
                data = new SignUpPostData
                {
                    
                    FirstName = "Unknown",
                    LastName = "Unknown"
                };
            }
            
            return new Result<TData>(true, (TData)data);
        }
    }
}