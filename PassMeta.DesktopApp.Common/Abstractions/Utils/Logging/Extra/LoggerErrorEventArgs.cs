using System;

namespace PassMeta.DesktopApp.Common.Abstractions.Utils.Logging.Extra;

/// <summary>
/// Information about an error that occurred during the operation of a logger.
/// </summary>
public class LoggerErrorEventArgs : EventArgs
{
    /// <summary>
    /// Error short description.
    /// </summary>
    public readonly string Message;

    /// <summary>
    /// Occured exception.
    /// </summary>
    public readonly Exception Exception;

    /// <summary></summary>
    public LoggerErrorEventArgs(string message, Exception ex)
    {
        Message = message;
        Exception = ex;
    }
}