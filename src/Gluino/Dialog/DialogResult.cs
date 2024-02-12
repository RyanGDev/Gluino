namespace Gluino;

/// <summary>Represents the result of a dialog operation.</summary>
public enum DialogResult
{
    /// <summary>The dialog operation resulted in an error.</summary>
    Error = -1,
    /// <summary>The dialog operation returned no result.</summary>
    None,
    /// <summary>The result value of the dialog operation is <see langword="OK"/> (usually sent from a button labeled OK).</summary>
    OK,
    /// <summary>The result value of the dialog operation is <see langword="Cancel"/> (usually sent from a button labeled Cancel).</summary>
    Cancel,
    /// <summary>The result value of the dialog operation is <see langword="Abort"/> (usually sent from a button labeled Abort).</summary>
    Abort,
    /// <summary>The result value of the dialog operation is <see langword="Retry"/> (usually sent from a button labeled Retry).</summary>
    Retry,
    /// <summary>The result value of the dialog operation is <see langword="Ignore"/> (usually sent from a button labeled Ignore).</summary>
    Ignore,
    /// <summary>The result value of the dialog operation is <see langword="Yes"/> (usually sent from a button labeled Yes).</summary>
    Yes,
    /// <summary>The result value of the dialog operation is <see langword="No"/> (usually sent from a button labeled No).</summary>
    No,
    /// <summary>The result value of the dialog operation is Try Again (usually sent from a button labeled Try Again).</summary>
    TryAgain,
    /// <summary>The result value of the dialog operation is Continue (usually sent from a button labeled Continue).</summary>
    Continue
}
