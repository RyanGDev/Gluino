namespace Gluino.Interop;

[LibDetails("Gluino.Core")]
internal partial class NativeMsgBox
{
    [LibImport("Gluino_MsgBox_Show")] 
    public static partial DialogResult Show(nint owner, string text, string title, MessageBoxButtons buttons, MessageBoxIcon icon);
}
