using Gluino.Interop;

namespace Gluino;

/// <summary>
/// Diaplays a dialog box that prompts the user to open a folder.
/// </summary>
public sealed class OpenFolderDialog : FileDialog
{
    /// <summary>
    /// Initializes a new instance of the <see cref="OpenFolderDialog"/> class.
    /// </summary>
    public OpenFolderDialog() : base(NativeFileDialogType.OpenFolder) { }

    /// <summary>
    /// Gets the selected folders.
    /// </summary>
    public string[] SelectedFolders => FileNames;

    /// <summary>
    /// Gets the selected folder.
    /// </summary>
    public string SelectedFolder => FileName;

    internal new bool MultiSelect {
        get => base.MultiSelect;
        set => base.MultiSelect = value;
    }

    internal new string[] FileNames => base.FileNames;

    internal new string FileName => base.FileName;
}
