using System.Runtime.InteropServices;

namespace Gluino.Interop;

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
internal struct NativeWebResourceResponse
{
    public string ContentType;
    public nint Content;
    [MarshalAs(UnmanagedType.I4)] public int ContentLength;
    [MarshalAs(UnmanagedType.I4)] public int StatusCode;
    public string ReasonPhrase;
}
