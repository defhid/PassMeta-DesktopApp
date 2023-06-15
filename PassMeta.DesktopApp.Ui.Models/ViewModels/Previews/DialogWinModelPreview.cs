using PassMeta.DesktopApp.Common.Enums;
using PassMeta.DesktopApp.Ui.Models.ViewModels.Windows.DialogWin;

namespace PassMeta.DesktopApp.Ui.Models.ViewModels.Previews;

/// <inheritdoc />
public class DialogWinModelPreview : DialogWinModel
{
    /// <summary></summary>
    public DialogWinModelPreview() : base(
        "Dialog Title",
        "Dialog Text",
        null,
        new[]
        {
            DialogButton.Yes,
            DialogButton.No,
            DialogButton.Cancel,
        },
        DialogWindowIcon.Hidden,
        null)
    {
    }
}