using System;

namespace PassMeta.DesktopApp.Ui.Models.DialogWindowModels;

public interface IInputBox
{
    Guid Tag { get; }
    
    bool Visible { get; }
}