using PassMeta.DesktopApp.Common.Enums;

namespace PassMeta.DesktopApp.Common.Abstractions.Utils;

/// <summary>
/// Passfile state manager.
/// </summary>
public interface IPassFileStateManager
{
    /// <summary>
    /// Get current passfile  mark (bit flags).
    /// </summary>
    PassFileMark GetMarks(int passFileId);

    /// <summary>
    /// Set actual mark (bit flags) to passfile.
    /// </summary>
    void SetMarks(int passFileId, PassFileMark mark);

    /// <summary>
    /// Clear all current passfile marks.
    /// </summary>
    void ResetMarks();

    /// <summary>
    /// Get current passfile passphrase.
    /// </summary>
    string? GetPassPhrase(int passFileId);
    
    /// <summary>
    /// Set actual passphrase to passfile.
    /// </summary>
    void SetPassPhrase(int passFileId, string? passPhrase);

    /// <summary>
    /// Clear all current passfile passphrases.
    /// </summary>
    void ResetPassPhrases();
}