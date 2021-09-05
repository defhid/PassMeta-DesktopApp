using System.Collections.Generic;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using PassMeta.DesktopApp.Common.Interfaces.Services;
using PassMeta.DesktopApp.Common.Models.Entities;
using PassMeta.DesktopApp.Common.Models.Entities.Request;
using PassMeta.DesktopApp.Common.Models.Entities.Response;
using PassMeta.DesktopApp.Core.Utils;
using PassMeta.DesktopApp.Ui.ViewModels;
using PassMeta.DesktopApp.Ui.Views.Base;
using Splat;

namespace PassMeta.DesktopApp.Ui.Views
{
    // ReSharper disable once UnusedType.Global
    public class AccountView : ViewPage<AccountViewModel>
    {
        private Dictionary<string, string> _whatMapper = new()
        {
            ["first_name"] = Common.Resources.ACCOUNT__FIRST_NAME_LABEL.ToLower(),
            ["last_name"] = Common.Resources.ACCOUNT__LAST_NAME_LABEL.ToLower(),
            ["login"] = Common.Resources.ACCOUNT__LOGIN_LABEL.ToLower(),
            ["password"] = Common.Resources.ACCOUNT__PASSWORD_LABEL.ToLower(),
            ["password_confirm"] = Common.Resources.ACCOUNT__PASSWORD_CONFIRM_LABEL.ToLower(),
        };

        public AccountView()
        {
            AvaloniaXamlLoader.Load(this);
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

            if (data.PasswordConfirm.Length == 0)
            {
                Locator.Current.GetService<IDialogService>()!.ShowFailure(Common.Resources.WARN__PASSWORD_CONFIRM_MISSED);
                return;
            }

            var response = await PassMetaApi.PatchAsync<UserPatchData, User>("/users/me", data);
            if (response is null) return;
            
            if (response.Success)
            {
                Locator.Current.GetService<IDialogService>()!.ShowInfo(Common.Resources.ACCOUNT__SAVE_SUCCESS);
                
                var user = response.Data;
                await AppConfig.Current.SetUserAsync(user);
                
                context.Refresh();
            }
            else
            {
                Locator.Current.GetService<IOkBadService>()!.ShowResponseFailure(response, _whatMapper);
            }
        }

        private async void SignOutBtn_OnClick(object? sender, RoutedEventArgs e)
        {
            var answer = await Locator.Current.GetService<IDialogService>()!
                .Confirm(Common.Resources.ACCOUNT__SIGN_OUT_CONFIRM);
            if (answer.Bad) return;
            
            var response = await PassMetaApi.PostAsync<Empty, Empty>("/auth/sign-out", new Empty(), true);
            if (response?.Success is not true) return;
            
            await AppConfig.Current.SetUserAsync(null);
            NavigateTo<AuthViewModel>();
        }
    }
}