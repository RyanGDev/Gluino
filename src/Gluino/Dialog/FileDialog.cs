using System.Runtime.InteropServices;
using Gluino.Interop;

namespace Gluino;

/// <summary>
/// Displays a dialog box from which the user can select a file.
/// </summary>
public abstract class FileDialog
{
    internal nint InstancePtr;
    internal NativeFileDialogInfo NativeOptions;

    internal FileDialog(NativeFileDialogType type)
    {
        InstancePtr = NativeFileDialog.Create(type);
        Title = type switch {
            NativeFileDialogType.OpenFile => "Open file",
            NativeFileDialogType.SaveFile => "Save file",
            NativeFileDialogType.OpenFolder => "Open folder",
            _ => null
        };
    }
    
    /// <summary>
    /// Gets or sets the title of the dialog.
    /// </summary>
    public string Title { get; set; }

    /// <summary>
    /// Gets or sets the default path of the dialog.
    /// </summary>
    public string DefaultPath { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the dialog allows multiple files to be selected.
    /// </summary>
    public bool MultiSelect { get; set; }

    /// <summary>
    /// Gets the collection of filters for the dialog.
    /// </summary>
    public FileDialogFilterCollection Filters { get; } = [];

    /// <summary>
    /// Gets the selected files.
    /// </summary>
    public string[] FileNames { get; private set; }

    /// <summary>
    /// Gets the selected file.
    /// </summary>
    public string FileName => FileNames?.Length > 0 ? FileNames[0] : null;

    /// <summary>
    /// Shows the dialog box with a default or specified owner window.
    /// </summary>
    /// <param name="owner">The window that will own the modal dialog box.</param>
    /// <returns>
    /// <see cref="DialogResult.OK"/> if the user clicks OK in the dialog box; otherwise, <see cref="DialogResult.Cancel"/>.
    /// </returns>
    public DialogResult ShowDialog(Window owner = null)
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

        NativeOptions.Title = Title;
        NativeOptions.DefaultPath = DefaultPath;
        NativeOptions.MultiSelect = MultiSelect;

        var filtersPtr = nint.Zero;
        if (Filters.Count > 0) {
            var filters = Filters.Select(f => f.ToNativeFilter()).ToArray();
            filtersPtr = Marshal.AllocHGlobal(Marshal.SizeOf<NativeFileDialogFilter>() * filters.Length);
            for (var i = 0; i < filters.Length; i++)
                Marshal.StructureToPtr(filters[i], filtersPtr + i * Marshal.SizeOf<NativeFileDialogFilter>(), false);

            NativeOptions.Filters = filtersPtr;
            NativeOptions.FilterCount = filters.Length;
        }

        var result = NativeFileDialog.ShowDialog(InstancePtr, owner.Handle, ref NativeOptions);

        if (filtersPtr != nint.Zero)
            Marshal.FreeHGlobal(filtersPtr);

        FileNames = new string[NativeOptions.FileNameCount];
        if (NativeOptions.FileNames != nint.Zero) {
            for (var i = 0; i < NativeOptions.FileNameCount; i++) {
                var strPtr = Marshal.ReadIntPtr(NativeOptions.FileNames, i * IntPtr.Size);
                FileNames[i] = Marshal.PtrToStringAuto(strPtr);
            }
        }

        return result;
    }
}
