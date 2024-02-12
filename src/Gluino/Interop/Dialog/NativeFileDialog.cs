namespace Gluino.Interop;

[LibDetails("Gluino.Core")]
internal partial class NativeFileDialog
{
    [LibImport("Gluino_FileDialog_Create")] public static partial nint Create(NativeFileDialogType type);
    [LibImport("Gluino_FileDialog_ShowDialog")] public static partial DialogResult ShowDialog(nint fileDialog, nint owner, ref NativeFileDialogInfo info);
}
