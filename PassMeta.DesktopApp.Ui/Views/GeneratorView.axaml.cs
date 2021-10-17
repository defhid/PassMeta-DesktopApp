using System;
using System.Threading.Tasks;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using PassMeta.DesktopApp.Common.Interfaces.Services;
using PassMeta.DesktopApp.Ui.ViewModels;
using PassMeta.DesktopApp.Ui.Views.Base;
using Splat;

namespace PassMeta.DesktopApp.Ui.Views
{
    // ReSharper disable once UnusedType.Global
    public class GeneratorView : ViewPage<GeneratorViewModel>
    {
        public GeneratorView()
        {
            AvaloniaXamlLoader.Load(this);
        }
        
        public override Task RefreshAsync()
        {
            throw new NotImplementedException();
        }

        private void GenerateBtn_OnClick(object? sender, RoutedEventArgs e)
        {
            var context = DataContext!;
            var service = Locator.Current.GetService<ICryptoService>()!;
            
            var password = service.GeneratePassword(context.Length, context.IncludeDigits, context.IncludeSpecial);
            context.Result = password;
            context.Generated = true;
        }

        private void CopyBtn_OnClick(object? sender, RoutedEventArgs e)
        {
            TextCopy.ClipboardService.SetText(DataContext!.Result ?? "");
        }
    }
}