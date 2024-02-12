using System.Runtime.InteropServices;

namespace Gluino.Interop;

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
internal struct NativeFileDialogFilter
{
    public string Name;
    public string Spec;
}
