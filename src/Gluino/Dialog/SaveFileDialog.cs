using Gluino.Interop;

namespace Gluino;

/// <summary>
/// Diaplays a dialog box that prompts the user to save a file.
/// </summary>
public class SaveFileDialog : FileDialog
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SaveFileDialog"/> class.
    /// </summary>
    public SaveFileDialog() : base(NativeFileDialogType.SaveFile) { }

    internal new bool MultiSelect {
        get => base.MultiSelect;
        set => base.MultiSelect = value;
    }

    internal new string[] FileNames => base.FileNames;
}
