namespace PassMeta.DesktopApp.Common.Enums;

/// <summary>
/// A way to show info to user.
/// </summary>
public enum DialogPresenter : byte
{
    /// <summary>
    /// Temporary notification.
    /// </summary>
    PopUp = 1,
        
    /// <summary>
    /// Independent dialog window.
    /// </summary>
    Window = 2
}