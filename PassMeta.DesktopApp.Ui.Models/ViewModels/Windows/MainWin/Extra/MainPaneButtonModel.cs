using System;
using System.Reactive.Linq;
using System.Windows.Input;
using Avalonia;
using Avalonia.Layout;
using Avalonia.Media;
using PassMeta.DesktopApp.Ui.Models.Constants;

namespace PassMeta.DesktopApp.Ui.Models.ViewModels.Windows.MainWin.Extra;

/// <summary>
/// A button for main pane.
/// </summary>
public class MainPaneButtonModel
{
    public IObservable<bool> IsActive { get; set; } = null!;

    public IObservable<bool> IsVisible { get; init; } = null!;

    public IObservable<string> Content { get; }

    public IObservable<int> Width { get; }

    public IObservable<Thickness> ButtonPadding { get; }

    public IObservable<HorizontalAlignment> HorizontalContentAlignment { get; }

    public IObservable<TextAlignment> HorizontalTextAlignment { get; }

    public IObservable<FontFamily> FontFamily { get; }

    public IObservable<int> FontSize { get; }

    public ICommand Command { get; set; } = null!;

    public MainPaneButtonModel(
        string text,
        string icon,
        IObservable<bool> shortMode)
    {
        Content = shortMode.Select(isShort => isShort ? icon : text);

        Width = shortMode.Select(isShort => isShort
            ? 40
            : 180);

        ButtonPadding = shortMode.Select(isShort => isShort
            ? Thickness.Parse("0 0 0 0")
            : Thickness.Parse("10 0 5 4"));

        HorizontalContentAlignment = shortMode.Select(isShort => isShort
            ? HorizontalAlignment.Center
            : HorizontalAlignment.Left);

        HorizontalTextAlignment = shortMode.Select(isShort => isShort
            ? TextAlignment.Center
            : TextAlignment.Left);

        FontFamily = shortMode.Select(isShort => isShort
            ? FontFamilies.SegoeMdl2
            : FontFamilies.Default);

        FontSize = shortMode.Select(isShort => isShort
            ? 28
            : 20);
    }
}