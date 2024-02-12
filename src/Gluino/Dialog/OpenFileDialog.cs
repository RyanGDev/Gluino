using Gluino.Interop;

namespace Gluino;

/// <summary>
/// Diaplays a dialog box that prompts the user to open a file.
/// </summary>
public sealed class OpenFileDialog : FileDialog
{
    /// <summary>
    /// Initializes a new instance of the <see cref="OpenFileDialog"/> class.
    /// </summary>
    public OpenFileDialog() : base(NativeFileDialogType.OpenFile) { }
}
