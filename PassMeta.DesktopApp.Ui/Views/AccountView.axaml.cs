using System;
using System.Threading.Tasks;
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
    public class AccountView : ViewPage<AccountViewModel>
    {
        public AccountView()
        {
            AvaloniaXamlLoader.Load(this);
        }
        
        public override Task RefreshAsync()
        {
            throw new NotImplementedException();
        }

        private async void SaveBtn_OnClick(object? sender, RoutedEventArgs e)
        {
            var context = DataContext!;
            var data = new UserPatchData
            {
                FirstName = context.FirstName ?? "",
                LastName = context.LastName ?? "",
                Login = string.IsNullOrEmpty(context.Login?.Trim()) ? null : context.Login.Trim(),
                Password = string.IsNullOrEmpty(context.Password) ? null : context.Password,
                PasswordConfirm = context.PasswordConfirm ?? "",
            };

            var service = Locator.Current.GetService<IAccountService>()!;
            var result = await service.UpdateUserData(data);
            
            if (result.Ok) context.Refresh();
        }

        private async void SignOutBtn_OnClick(object? sender, RoutedEventArgs e)
        {
            var service = Locator.Current.GetService<IAuthService>()!;
            await service.SignOutAsync();
            NavigateTo<AuthViewModel>();
        }
    }
}