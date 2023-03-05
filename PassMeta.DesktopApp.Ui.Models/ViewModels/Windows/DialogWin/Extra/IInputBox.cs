using System;

namespace PassMeta.DesktopApp.Ui.Models.ViewModels.Windows.DialogWin;

public interface IInputBox
{
    Guid Tag { get; }
    
    bool Visible { get; }
}