using Gluino.Interop;

namespace Gluino;

/// <summary>
/// Displays a message box that can contain text, buttons, and an icon that inform and instruct the user.
/// </summary>
public static class MessageBox
{
    /// <summary>
    /// Displays a message box in front of the specified <see cref="Window"/> and with the specified text, title, buttons, and icon.
    /// </summary>
    /// <param name="owner">A <see cref="Window"/> that will own the modal dialog box.</param>
    /// <param name="text">The text to display in the message box.</param>
    /// <param name="caption">The text to display in the title bar of the message box.</param>
    /// <param name="buttons">One of the <see cref="MessageBoxButtons"/> values that specifies which buttons to display in the message box.</param>
    /// <param name="icon">One of the <see cref="MessageBoxIcon"/> values that specifies which icon to display in the message box.</param>
    /// <returns>One of the <see cref="DialogResult"/> values.</returns>
    public static DialogResult Show(Window owner, string text, string caption, MessageBoxButtons buttons, MessageBoxIcon icon)
    {
        if (owner != null && owner.InstancePtr == nint.Zero) {
            throw new Exception("The owner window must be created before the dialog can be shown.");
        }

        if (owner == null) {
            owner = App.MainWindow;

            if (owner == null)
                throw new Exception(
                    "Owner window not provided.\nThe application's main window must be initialized before the dialog can be shown.");

            if (owner.InstancePtr == nint.Zero)
                throw new Exception(
                    "Owner window not provided.\nThe application's main window must be created before the dialog can be shown.");
        }

        return NativeMsgBox.Show(owner.Handle, text, caption, buttons, icon);
    }

    /// <summary>
    /// Displays a message box in front of the specified <see cref="Window"/> and with the specified text, title, and buttons.
    /// </summary>
    /// <inheritdoc cref="Show(Window, string, string, MessageBoxButtons, MessageBoxIcon)"/>
    public static DialogResult Show(Window owner, string text, string caption, MessageBoxButtons buttons) =>
        Show(owner, text, caption, buttons, MessageBoxIcon.None);

    /// <summary>
    /// Displays a message box in front of the specified <see cref="Window"/> and with the specified text and title.
    /// </summary>
    /// <inheritdoc cref="Show(Window, string, string, MessageBoxButtons, MessageBoxIcon)"/>
    public static DialogResult Show(Window owner, string text, string caption) =>
        Show(owner, text, caption, MessageBoxButtons.OK, MessageBoxIcon.None);

    /// <summary>
    /// Displays a message box in front of the specified <see cref="Window"/> and with the specified text.
    /// </summary>
    /// <inheritdoc cref="Show(Window, string, string, MessageBoxButtons, MessageBoxIcon)"/>
    public static DialogResult Show(Window owner, string text) =>
        Show(owner, text, string.Empty, MessageBoxButtons.OK, MessageBoxIcon.None);

    /// <summary>
    /// Displays a message box with specified text, title, buttons, and icon.
    /// </summary>
    /// <inheritdoc cref="Show(Window, string, string, MessageBoxButtons, MessageBoxIcon)"/>
    public static DialogResult Show(string text, string caption, MessageBoxButtons buttons, MessageBoxIcon icon) =>
        Show(null, text, caption, buttons, icon);

    /// <summary>
    /// Diaplays a message box with specified text, title, and buttons.
    /// </summary>
    /// <inheritdoc cref="Show(Window, string, string, MessageBoxButtons, MessageBoxIcon)"/>
    public static DialogResult Show(string text, string caption, MessageBoxButtons buttons) =>
        Show(null, text, caption, buttons, MessageBoxIcon.None);

    /// <summary>
    /// Displays a message box with specified text and title.
    /// </summary>
    /// <inheritdoc cref="Show(Window, string, string, MessageBoxButtons, MessageBoxIcon)"/>
    public static DialogResult Show(string text, string caption) =>
        Show(null, text, caption, MessageBoxButtons.OK, MessageBoxIcon.None);

    /// <summary>
    /// Displays a message box with specified text.
    /// </summary>
    /// <inheritdoc cref="Show(Window, string, string, MessageBoxButtons, MessageBoxIcon)"/>
    public static DialogResult Show(string text) =>
        Show(null, text, string.Empty, MessageBoxButtons.OK, MessageBoxIcon.None);
}
