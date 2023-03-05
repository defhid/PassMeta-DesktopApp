using PassMeta.DesktopApp.Common.Abstractions.Utils.ValueMapping;
using PassMeta.DesktopApp.Common.Enums;
using PassMeta.DesktopApp.Common.Utils.ValueMapping;

namespace PassMeta.DesktopApp.Common.Mapping.Values;

/// <summary>
/// Mappers for <see cref="DialogButton"/>.
/// </summary>
public static class DialogButtonMapping
{
    private static readonly ValuesMapper<DialogButton, string> ButtonToNameMapper = new MapToResource<DialogButton>[]
    {
        new(DialogButton.Ok, () => Resources.DIALOG__BTN_OK),
        new(DialogButton.Yes, () => Resources.DIALOG__BTN_YES),
        new(DialogButton.No, () => Resources.DIALOG__BTN_NO),
        new(DialogButton.Cancel, () => Resources.DIALOG__BTN_CANCEL),
        new(DialogButton.Close, () => Resources.DIALOG__BTN_CLOSE),
    };

    /// <summary>
    /// Mapper for <see cref="DialogButton"/> names.
    /// </summary>
    public static IValuesMapper<DialogButton, string> ButtonToName => ButtonToNameMapper;
}