using System.Runtime.InteropServices;

namespace Gluino.Interop;

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
internal struct NativeFileDialogInfo
{
    public string Title;
    public string DefaultPath;
    [MarshalAs(UnmanagedType.I1)] public bool MultiSelect;
    public nint Filters;
    [MarshalAs(UnmanagedType.I4)] public int FilterCount;
    public nint FileNames;
    [MarshalAs(UnmanagedType.I4)] public int FileNameCount;
}
