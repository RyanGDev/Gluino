using System.Runtime.InteropServices;

namespace Gluino.Interop;

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
internal struct NativeWebViewOptions
{
    public string UserDataPath;
    public string StartUrl;
    public string StartContent;
    [MarshalAs(UnmanagedType.I1)] public bool ContextMenuEnabled;
    [MarshalAs(UnmanagedType.I1)] public bool DevToolsEnabled;
    public string UserAgent;
    [MarshalAs(UnmanagedType.I1)] public bool GrantPermissions;
}