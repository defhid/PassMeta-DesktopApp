namespace PassMeta.DesktopApp.Ui.ViewModels.Components;

using System;
using System.Reactive;
using System.Windows.Input;
using Avalonia.Media;
using ReactiveUI;

public class BtnState : ReactiveObject
{
    #region Content

    private readonly ObservableAsPropertyHelper<string>? _content;
    public IObservable<string> ContentObservable
    {
        init => _content = value.ToProperty(this, nameof(Content));
    }
    public string Content => _content!.Value;

    #endregion

    #region Command

    private readonly ObservableAsPropertyHelper<ReactiveCommand<Unit, Unit>>? _command;
    public IObservable<ReactiveCommand<Unit, Unit>> CommandObservable
    {
        init => _command = value.ToProperty(this, nameof(Command));
    }
    public ICommand Command => _command!.Value;

    #endregion

    #region IsVisible

    private readonly ObservableAsPropertyHelper<bool>? _isVisible;
    public IObservable<bool> IsVisibleObservable
    {
        init => _isVisible = value.ToProperty(this, nameof(IsVisible));
    }
    public bool IsVisible => _isVisible!.Value;

    #endregion

    #region Opacity

    private readonly ObservableAsPropertyHelper<double>? _opacity;
    public IObservable<double> OpacityObservable
    {
        init => _opacity = value.ToProperty(this, nameof(Opacity));
    }
    public double Opacity => _opacity!.Value;

    #endregion

    #region FintSize

    private readonly ObservableAsPropertyHelper<double>? _fontSize;
    public IObservable<double> FontSizeObservable
    {
        init => _fontSize = value.ToProperty(this, nameof(FontSize));
    }
    public double FontSize => _fontSize!.Value;

    #endregion

    #region FontFamily

    private readonly ObservableAsPropertyHelper<FontFamily>? _fontFamily;
    public IObservable<FontFamily> FontFamilyObservable
    {
        init => _fontFamily = value.ToProperty(this, nameof(FontFamily));
    }
    public FontFamily FontFamily => _fontFamily!.Value;

    #endregion
        
    #region FintSize

    private readonly ObservableAsPropertyHelper<int>? _rotationAngle;
    public IObservable<int> RotationAngleObservable
    {
        init => _rotationAngle = value.ToProperty(this, nameof(RotationAngle));
    }
    public double RotationAngle => _rotationAngle!.Value;

    #endregion
}