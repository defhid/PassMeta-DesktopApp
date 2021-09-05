using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using PassMeta.DesktopApp.Common.Interfaces.Services;
using PassMeta.DesktopApp.Common.Models;
using PassMeta.DesktopApp.Common.Models.Entities;
using PassMeta.DesktopApp.Common.Models.Entities.Request;
using PassMeta.DesktopApp.Core.Utils;
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

        private async void SignInBtn_OnClick(object? sender, RoutedEventArgs e)
        {
            await SignInAsync(false);
        }
        
        private async void SignUpBtn_OnClick(object? sender, RoutedEventArgs e)
        {
            var result = ValidateAndGetData<SignUpPostData>();
            if (!result.Ok)
            {
                Locator.Current.GetService<IDialogService>()!.ShowError(Common.Resources.ERR__DATA_VALIDATION);
            }
            else
            {
                var response = await PassMetaApi.PostAsync<SignUpPostData, User>("/users/new", result.Data, true);
                if (response?.Success is not true) return;
                
                await AppConfig.Current.SetUserAsync(response.Data);
                NavigateTo<AccountViewModel>();
            }
        }

        private async Task SignInAsync(bool silent)
        {
            var result = ValidateAndGetData<SignInPostData>();
            if (!result.Ok)
            {
                if (silent) return;
                Locator.Current.GetService<IDialogService>()!.ShowError(Common.Resources.ERR__DATA_VALIDATION);
            }
            else
            {
                var response = await PassMetaApi.PostAsync<SignInPostData, User>("/auth/sign-in", result.Data, true);
                if (response?.Success is not true) return;
                
                await AppConfig.Current.SetUserAsync(response.Data);
                NavigateTo<AccountViewModel>();
            }
        }

        private Result<TData> ValidateAndGetData<TData>()
            where TData : SignInPostData
        {
            var context = DataContext!;
            SignInPostData data;
            
            if (typeof(TData) == typeof(SignInPostData))
            {
                data = new SignInPostData
                {
                    Login = context.Login?.Trim() ?? "",
                    Password = context.Password ?? ""
                };
            }
            else
            {
                data = new SignUpPostData
                {
                    Login = context.Login?.Trim() ?? "",
                    Password = context.Password ?? "",
                    FirstName = "Unknown",
                    LastName = "Unknown"
                };
            }

            if (data.Login.Length < 1)
                return new Result<TData>(false);

            if (data.Password.Length < 2)
                return new Result<TData>(false);
            
            return new Result<TData>(data);
        }

        private async void Input_OnKeyDown(object? sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter) 
                await SignInAsync(true);
        }

        private void LoginTextBox__OnAttachedToVisualTree(object? sender, VisualTreeAttachmentEventArgs e)
        {
            (sender as TextBox)?.Focus();
        }
    }
}