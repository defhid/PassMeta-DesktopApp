using Avalonia.Controls;
using Avalonia.Input;
using PassMeta.DesktopApp.Common.Abstractions.Services;
using PassMeta.DesktopApp.Common.Extensions;
using PassMeta.DesktopApp.Ui.Models.ViewModels.Pages.StoragePage.Components;
using Splat;

namespace PassMeta.DesktopApp.Ui.Views.Pages.Storage.Components;

public partial class PwdSectionReadView : UserControl
{
    public PwdSectionReadView()
    {
        InitializeComponent();
    }

    private async void WebsiteUrlTextBlock_OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        var url = "https://" + ((PwdSectionReadModel)DataContext!).WebsiteUrl;

        if (await Locator.Current.Resolve<IClipboardService>().TrySetTextAsync(url))
        {
            Locator.Current.Resolve<IDialogService>()
                .ShowInfo(Common.Resources.STORAGE__ITEM_INFO__WEBSITE_COPIED);
        }
    }
}